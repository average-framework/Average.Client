using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using Newtonsoft.Json;
using System;
using static Average.Shared.SharedAPI;

namespace Average.Client.Framework.Menu
{
    internal class ButtonItem : IPrimaryMenuItem
    {
        public string Id { get; private set; }
        public string Text { get; set; }
        public bool Visible { get; set; }
        public bool Disabled { get; set; }

        [JsonIgnore]
        public Action<ButtonItem> OnClick { get; }

        public ButtonItem(string text, Action<ButtonItem> onClick, bool visible = true, bool disabled = false)
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
