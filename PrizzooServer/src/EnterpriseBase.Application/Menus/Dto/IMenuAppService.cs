using Abp.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseBase.Menus.Dto
{
    public interface IMenuAppService : IApplicationService
    {
        Task<MenuDto> GetUserMenu();
        Task<List<string>> GetMyGrantedPermissions();
    }
}
