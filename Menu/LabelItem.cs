using Average.Client.Framework.Services;
using Average.Client.Interfaces;
using static Average.Shared.SharedAPI;

namespace Average.Client.Menu
{
    internal class LabelItem : IMenuItem
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public bool Visible { get; set; }
        public MenuContainer Parent { get; set; }

        public LabelItem(string text, bool visible = true)
        {
            Name = RandomString();
            Text = text;
            Visible = visible;
        }

        public void OnRender(UIService uiService) => uiService.SendNui("menu", "render_item", new
        {
            type = "label",
            name = Name,
            text = Text,
            visible = Visible
        });
    }
}
