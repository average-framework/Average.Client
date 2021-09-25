using Average.Client.Framework.Services;
using Average.Client.Interfaces;
using System;
using static Average.Shared.SharedAPI;

namespace Average.Client.Menu
{
    internal class ButtonItem : IMenuItem
    {
        public string Name { get; }
        public string Text { get; set; }
        public bool Visible { get; set; }
        public MenuContainer Parent { get; set; }
        public Action<ButtonItem> Action { get; }

        public ButtonItem(string text, Action<ButtonItem> action, bool visible = true)
        {
            Name = RandomString();
            Text = text;
            Action = action;
            Visible = visible;
        }

        public void OnRender(UIService uiService) => uiService.SendNui("menu", "render_item", new
        {
            type = "button",
            name = Name,
            text = Text,
            visible = Visible
        });
    }
}
