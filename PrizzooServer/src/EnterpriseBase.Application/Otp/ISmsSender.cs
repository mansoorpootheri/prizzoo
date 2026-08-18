using Abp.Dependency;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EnterpriseBase.Application.Otp
{
    public interface ISmsSender
    {
        Task SendOtpAsync(string phoneNumber, string code);
    }

    /// <summary>
    /// TODO: replace with a real SMS gateway (e.g. MSG91/Twilio) before any
    /// real phone number is used. This implementation only logs the code so
    /// it can be read from server output during development - it sends
    /// nothing to the phone.
    /// </summary>
    public class NoopSmsSender : ISmsSender, ITransientDependency
    {
        private readonly ILogger<NoopSmsSender> _logger;

        public NoopSmsSender(ILogger<NoopSmsSender> logger)
        {
            _logger = logger;
        }

        public Task SendOtpAsync(string phoneNumber, string code)
        {
            _logger.LogWarning(
                "[DEV OTP - NoopSmsSender, not a real SMS gateway] {PhoneNumber} -> {Code}",
                phoneNumber, code);
            return Task.CompletedTask;
        }
    }
}
