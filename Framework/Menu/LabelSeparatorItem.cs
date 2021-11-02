using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using static Average.Shared.SharedAPI;

namespace Average.Client.Framework.Menu
{
    internal class LabelSeparatorItem : IPrimaryMenuItem
    {
        public string Id { get; private set; }
        public string Text { get; set; }
        public string Ico { get; set; }
        public bool Visible { get; set; }
        public bool Disabled { get; set; }

        public LabelSeparatorItem(string text, string ico, bool visible = true, bool disabled = false)
        {
            Id = RandomString();
            Text = text;
            Ico = ico;
            Visible = visible;
            Disabled = disabled;
        }

        public object OnRender() => new
        {
            type = GetType().Name,
            id = Id,
            text = Text,
            ico = Ico,
            visible = Visible,
            disabled = Disabled
        };

        public void OnUpdate(UIService uiService) => uiService.SendNui("menu", "render_item", OnRender());
    }
}
