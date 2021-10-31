using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using static Average.Shared.SharedAPI;

namespace Average.Client.Framework.Menu
{
    internal class LabelItem : ISecondaryMenuItem
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public bool Visible { get; set; } = true;
        public bool Disabled { get; set; }

        public LabelItem(string text, bool visible = true, bool disabled = false)
        {
            Id = RandomString();
            Text = text;
            Visible = visible;
            Disabled = disabled;
        }

        public object OnRender() => new
        {
            type = GetType().Name,
            id = Id,
            text = Text,
            visible = Visible,
            disabled = Disabled
        };

        public void OnUpdate(UIService uiService) => uiService.SendNui("menu", "render_item", OnRender());
    }
}
