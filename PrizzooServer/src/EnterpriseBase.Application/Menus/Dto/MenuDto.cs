using System.Collections.Generic;

namespace EnterpriseBase.Menus.Dto
{
    public class MenuDto
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public List<MenuItemDto> Items { get; set; } = new List<MenuItemDto>();
    }

    public class MenuItemDto
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Icon { get; set; }
        public string Url { get; set; }

        public List<MenuItemDto> Items { get; set; } = new List<MenuItemDto>();
    }
}
