using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using Newtonsoft.Json;
using System;
using static Average.Client.Framework.GameAPI;

namespace Average.Client.Framework.Menu
{
    internal class BottomButtonItem : ISecondaryMenuItem
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public bool Visible { get; set; } = true;
        public bool Disabled { get; set; }

        [JsonIgnore]
        public Action<BottomButtonItem> OnClick { get; }

        public BottomButtonItem(string text, Action<BottomButtonItem> onClick, bool visible = true, bool disabled = false)
        {
            Id = RandomString();
            Text = text;
            OnClick = onClick;
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
