using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using EnterpriseBase.Application.ReleaseNotes.Dto;
using EnterpriseBase.ReleaseNotes;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseBase.Application.ReleaseNotes
{
    public class ReleaseNoteAppService : ApplicationService, IReleaseNoteAppService
    {
        private readonly IRepository<ReleaseNote, Guid> _repository;

        public ReleaseNoteAppService(IRepository<ReleaseNote, Guid> repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Get the latest active release note. Any authenticated user can call this.
        /// </summary>
        [AbpAuthorize]
        public async Task<ReleaseNoteDto> GetLatest()
        {
            var note = await _repository.GetAll()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.ReleaseDate)
                .FirstOrDefaultAsync();

            if (note == null) return null;

            return MapToDto(note);
        }

        /// <summary>
        /// Get all release notes (for history view). Any authenticated user can call this.
        /// </summary>
        [AbpAuthorize]
        public async Task<List<ReleaseNoteDto>> GetAll()
        {
            var notes = await _repository.GetAll()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.ReleaseDate)
                .ToListAsync();

            return notes.Select(MapToDto).ToList();
        }

        /// <summary>
        /// Get all release notes including inactive (admin view).
        /// </summary>
        [AbpAuthorize("Pages.Administration.Host.Settings")]
        public async Task<List<ReleaseNoteDto>> GetAllForAdmin()
        {
            var notes = await _repository.GetAll()
                .OrderByDescending(x => x.ReleaseDate)
                .ToListAsync();

            return notes.Select(MapToDto).ToList();
        }

        /// <summary>
        /// Create a release note (host admin only).
        /// </summary>
        [AbpAuthorize("Pages.Administration.Host.Settings")]
        public async Task<ReleaseNoteDto> Create(CreateReleaseNoteDto input)
        {
            var note = new ReleaseNote
            {
                Version = input.Version,
                Title = input.Title,
                ReleaseDate = input.ReleaseDate,
                Features = System.Text.Json.JsonSerializer.Serialize(input.Features),
                IsActive = input.IsActive,
            };

            await _repository.InsertAsync(note);
            await CurrentUnitOfWork.SaveChangesAsync();

            return MapToDto(note);
        }

        /// <summary>
        /// Update a release note (host admin only).
        /// </summary>
        [AbpAuthorize("Pages.Administration.Host.Settings")]
        public async Task<ReleaseNoteDto> Update(UpdateReleaseNoteDto input)
        {
            var note = await _repository.GetAsync(input.Id);

            note.Version = input.Version;
            note.Title = input.Title;
            note.ReleaseDate = input.ReleaseDate;
            note.Features = System.Text.Json.JsonSerializer.Serialize(input.Features);
            note.IsActive = input.IsActive;

            await _repository.UpdateAsync(note);

            return MapToDto(note);
        }

        /// <summary>
        /// Delete a release note (host admin only).
        /// </summary>
        [AbpAuthorize("Pages.Administration.Host.Settings")]
        public async Task Delete(Guid id)
        {
            await _repository.DeleteAsync(id);
        }

        private ReleaseNoteDto MapToDto(ReleaseNote note)
        {
            List<string> features;
            try
            {
                features = System.Text.Json.JsonSerializer.Deserialize<List<string>>(note.Features) ?? new List<string>();
            }
            catch
            {
                features = new List<string>();
            }

            return new ReleaseNoteDto
            {
                Id = note.Id,
                Version = note.Version,
                Title = note.Title,
                ReleaseDate = note.ReleaseDate,
                Features = features,
                IsActive = note.IsActive,
                CreationTime = note.CreationTime,
            };
        }
    }
}
