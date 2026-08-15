using Abp.Application.Navigation;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Localization;
using Abp.Runtime.Session;
using Abp.UI;
using EnterpriseBase.Menus.Dto;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseBase.Menus
{
    public class MenuAppService : ApplicationService, IMenuAppService
    {
        private readonly IUserNavigationManager _userNavigationManager;
        private readonly IPermissionChecker _permissionChecker;
        private readonly IPermissionManager _permissionManager;

        public MenuAppService(
            IUserNavigationManager userNavigationManager,
            IPermissionChecker permissionChecker,
            IPermissionManager permissionManager)
        {
            _userNavigationManager = userNavigationManager;
            _permissionChecker = permissionChecker;
            _permissionManager = permissionManager;
        }

        public async Task<MenuDto> GetUserMenu()
        {
            // Get menu filtered by permissions of CURRENT USER
            var userMenu = await _userNavigationManager.GetMenuAsync(
                "MainMenu",
                AbpSession.ToUserIdentifier()
            );

            if (userMenu == null)
            {
                throw new UserFriendlyException("MainMenu is not defined.");
            }

            return MapMenu(userMenu);
        }

        private MenuDto MapMenu(UserMenu menu)
        {
            return new MenuDto
            {
                Name = menu.Name,
                DisplayName = menu.DisplayName?.ToString(),
                Items = menu.Items.Select(MapMenuItem).ToList()
            };
        }

        private MenuItemDto MapMenuItem(UserMenuItem item)
        {
            return new MenuItemDto
            {
                Name = item.Name,
                DisplayName = item.DisplayName?.ToString(),
                Icon = item.Icon,
                Url = item.Url,
                Items = item.Items.Select(MapMenuItem).ToList()
            };
        }

        public async Task<List<string>> GetMyGrantedPermissions()
        {
            var result = new List<string>();

            foreach (var permission in _permissionManager.GetAllPermissions())
            {
                if (await _permissionChecker.IsGrantedAsync(permission.Name))
                {
                    result.Add(permission.Name);
                }
            }

            return result;
        }
    }
}
