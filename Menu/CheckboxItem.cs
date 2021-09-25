using Average.Client.Framework.Services;
using Average.Client.Interfaces;
using System;
using static Average.Shared.SharedAPI;

namespace Average.Client.Menu
{
    internal class CheckboxItem : IMenuItem
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public bool Checked { get; set; }
        public bool Visible { get; set; }
        public MenuContainer Parent { get; set; }
        public Action<CheckboxItem> Action { get; }

        public CheckboxItem(string text, bool @checked, Action<CheckboxItem> action, bool visible = true)
        {
            Name = RandomString();
            Text = text;
            Checked = @checked;
            Action = action;
            Visible = visible;
        }

        public void OnRender(UIService uiService) => uiService.SendNui("menu", "render_item", new
        {
            type = "checkbox",
            name = Name,
            text = Text,
            isChecked = Checked,
            visible = Visible
        });
    }
}
