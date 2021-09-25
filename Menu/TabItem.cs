using Average.Client.Framework.Services;
using System;
using static Average.Shared.SharedAPI;

namespace Average.Client.Menu
{
    internal class TabItem
    {
        public string Name { get; }
        public string IconPath { get; set; }
        public bool Visible { get; set; }
        public bool IsSelected { get; set; }
        public MenuContainer TargetContainer { get; }
        public Action<TabItem> Action { get; }

        public TabItem(string iconPath, Action<TabItem> action, MenuContainer targetContainer, bool isSelected = false, bool visible = true)
        {
            Name = RandomString();
            IconPath = iconPath;
            Action = action;
            IsSelected = isSelected;
            Visible = visible;
            TargetContainer = targetContainer;
        }

        public void OnRender(UIService uiService) => uiService.SendNui("menu", "render_item", new
        {
            type = "selector",
            name = Name,
            iconpath = IconPath,
            visible = Visible,
            isSelected = IsSelected
        });
    }
}
