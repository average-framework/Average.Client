using Average.Client.Framework.Services;
using Average.Client.Interfaces;
using System;
using static Average.Shared.SharedAPI;

namespace Average.Client.Menu
{
    internal class ButtonContainer : IMenuItem
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public bool Visible { get; set; }
        public MenuContainer Parent { get; set; }
        public MenuContainer Target { get; }
        public Action<ButtonContainer> Action { get; }

        public ButtonContainer(string text, MenuContainer target, Action<ButtonContainer> action = null, bool visible = true)
        {
            Name = RandomString();
            Text = text;
            Target = target;
            Action = action;
            Visible = visible;
        }

        public void OnRender(UIService uiService) => uiService.SendNui("menu", "render_item", new
        {
            type = "button_container",
            name = Name,
            text = Text,
            visible = Visible
        });
    }
}
