using Average.Client.Framework.Services;
using Newtonsoft.Json;
using System;
using static Average.Shared.SharedAPI;

namespace Average.Client.Framework.Menu
{
    internal class TextboxItem : IPrimaryMenuItem
    {
        public string Id { get; private set; }
        public string Text { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public string PlaceHolder { get; set; }
        public object Value { get; set; }
        public bool Visible { get; set; }

        [JsonIgnore]
        public Action<TextboxItem, object> OnInput { get; }

        public TextboxItem(string text, int minLength, int maxLength, string placeHolder, object value, Action<TextboxItem, object> onInput, bool visible = true)
        {
            Id = RandomString();
            Text = text;
            MinLength = minLength;
            MaxLength = maxLength;
            PlaceHolder = placeHolder;
            Value = value;
            OnInput = onInput;
            Visible = visible;
        }

        public object OnRender() => new
        {
            type = GetType().Name,
            id = Id,
            text = Text,
            minLength = MinLength,
            maxLength = MaxLength,
            placeHolder = PlaceHolder,
            value = Value,
            visible = Visible
        };

        public void OnUpdate(UIService uiService) => uiService.SendNui("menu", "render_item", OnRender());
    }
}
