using Average.Client.Framework.Services;
using Average.Client.Interfaces;
using System;
using static Average.Shared.SharedAPI;

namespace Average.Client.Menu
{
    internal class TextBoxItem : IMenuItem
    {
        public string Name { get; }
        public string Text { get; set; }
        public string Placeholder { get; set; }
        public string Pattern { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public object Value { get; set; }
        public bool Visible { get; set; }
        public MenuContainer Parent { get; set; }
        public Action<object> Action { get; private set; }

        public TextBoxItem(string text, object value, string placeholder, string pattern, int minLength, int maxLength, Action<object> action, bool visible = true)
        {
            Name = RandomString();
            Text = text;
            Value = value;
            Placeholder = placeholder;
            Pattern = pattern;
            MinLength = minLength;
            MaxLength = maxLength;
            Action = action;
            Visible = visible;
        }

        public void OnRender(UIService uiService) => uiService.SendNui("menu", "render_item", new
        {
            type = "textbox",
            name = Name,
            text = Text,
            value = Value,
            placeholder = Placeholder,
            pattern = Pattern,
            minLength = MinLength,
            maxLength = MaxLength,
            visible = Visible
        });
    }
}
