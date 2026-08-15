using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using EnterpriseBase.Application.ReleaseNotes.Dto;

namespace EnterpriseBase.Application.ReleaseNotes
{
    public interface IReleaseNoteAppService : IApplicationService
    {
        Task<ReleaseNoteDto> GetLatest();
        Task<List<ReleaseNoteDto>> GetAll();
        Task<List<ReleaseNoteDto>> GetAllForAdmin();
        Task<ReleaseNoteDto> Create(CreateReleaseNoteDto input);
        Task<ReleaseNoteDto> Update(UpdateReleaseNoteDto input);
        Task Delete(Guid id);
    }
}
