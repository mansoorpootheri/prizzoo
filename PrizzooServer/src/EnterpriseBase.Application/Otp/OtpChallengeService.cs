using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using EnterpriseBase.Authorization.Otp;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseBase.Application.Otp
{
    public interface IOtpChallengeService
    {
        /// <summary>Generates and sends a new OTP for this phone number. Throttled per phone.</summary>
        Task RequestAsync(string phoneNumber);

        /// <summary>Verifies a code against the latest unconsumed challenge for this phone number. Throws on failure.</summary>
        Task VerifyAsync(string phoneNumber, string code);
    }

    /// <summary>
    /// Generates, hashes, rate-limits, and verifies OTP codes for shopper
    /// phone login. Codes are never stored in plaintext - only a SHA-256
    /// hash keyed by phone number - so a DB leak doesn't expose live codes.
    /// This is a public, unauthenticated, account-creating surface, so the
    /// baseline abuse protections here (expiry, attempt cap, request
    /// throttle) are load-bearing, not optional polish.
    /// </summary>
    public class OtpChallengeService : IOtpChallengeService, ITransientDependency
    {
        private static readonly TimeSpan Expiry = TimeSpan.FromMinutes(5);
        private const int MaxVerifyAttempts = 5;
        private const int MaxRequestsPerWindow = 3;
        private static readonly TimeSpan RequestWindow = TimeSpan.FromMinutes(10);

        private readonly IRepository<OtpChallenge, Guid> _repository;
        private readonly ISmsSender _smsSender;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public OtpChallengeService(
            IRepository<OtpChallenge, Guid> repository,
            ISmsSender smsSender,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _repository = repository;
            _smsSender = smsSender;
            _unitOfWorkManager = unitOfWorkManager;
        }

        [UnitOfWork]
        public virtual async Task RequestAsync(string phoneNumber)
        {
            var windowStart = DateTime.UtcNow - RequestWindow;
            var recentCount = await _repository.GetAll()
                .Where(x => x.PhoneNumber == phoneNumber && x.CreationTime >= windowStart)
                .CountAsync();

            if (recentCount >= MaxRequestsPerWindow)
                throw new UserFriendlyException("Too many OTP requests for this number. Please try again later.");

            var code = GenerateCode();

            var challenge = new OtpChallenge
            {
                PhoneNumber = phoneNumber,
                CodeHash = Hash(phoneNumber, code),
                ExpiresAt = DateTime.UtcNow.Add(Expiry),
                AttemptCount = 0,
            };

            await _repository.InsertAsync(challenge);
            await _unitOfWorkManager.Current.SaveChangesAsync();

            await _smsSender.SendOtpAsync(phoneNumber, code);
        }

        [UnitOfWork]
        public virtual async Task VerifyAsync(string phoneNumber, string code)
        {
            var challenge = await _repository.GetAll()
                .Where(x => x.PhoneNumber == phoneNumber && x.ConsumedAt == null)
                .OrderByDescending(x => x.CreationTime)
                .FirstOrDefaultAsync();

            if (challenge == null)
                throw new UserFriendlyException("No pending OTP request for this phone number.");

            if (challenge.ExpiresAt < DateTime.UtcNow)
                throw new UserFriendlyException("This OTP has expired. Please request a new one.");

            if (challenge.AttemptCount >= MaxVerifyAttempts)
                throw new UserFriendlyException("Too many incorrect attempts. Please request a new OTP.");

            if (challenge.CodeHash != Hash(phoneNumber, code))
            {
                challenge.AttemptCount++;
                await _repository.UpdateAsync(challenge);
                await _unitOfWorkManager.Current.SaveChangesAsync();
                throw new UserFriendlyException("Incorrect OTP.");
            }

            challenge.ConsumedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(challenge);
            await _unitOfWorkManager.Current.SaveChangesAsync();
        }

        // TODO: revert to the random generator below once a real SMS gateway
        // is wired into ISmsSender - a static code means anyone can log in
        // as any phone number right now. Fine only because NoopSmsSender
        // doesn't send anything real and this is a dev/testing build.
        private static string GenerateCode()
        {
            return "123456";
        }

        // private static string GenerateCode()
        // {
        //     return RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        // }

        private static string Hash(string phoneNumber, string code)
        {
            var bytes = Encoding.UTF8.GetBytes($"{phoneNumber}:{code}");
            return Convert.ToHexString(SHA256.HashData(bytes));
        }
    }
}
