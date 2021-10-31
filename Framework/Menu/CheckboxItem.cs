using Average.Client.Framework.Services;
using Newtonsoft.Json;
using System;
using static Average.Shared.SharedAPI;

namespace Average.Client.Framework.Menu
{
    internal class CheckboxItem : IPrimaryMenuItem
    {
        public string Id { get; private set; }
        public string Text { get; set; }
        public bool IsChecked { get; set; }
        public bool Visible { get; set; }

        [JsonIgnore]
        public Action<CheckboxItem, bool> OnChange { get; }

        public CheckboxItem(string text, bool isChecked, Action<CheckboxItem, bool> onChange, bool visible = true)
        {
            Id = RandomString();
            Text = text;
            IsChecked = isChecked;
            OnChange = onChange;
            Visible = visible;
        }

        public object OnRender() => new
        {
            type = GetType().Name,
            id = Id,
            text = Text,
            isChecked = IsChecked,
            visible = Visible
        };

        public void OnUpdate(UIService uiService) => uiService.SendNui("menu", "render_item", OnRender());
    }
}
