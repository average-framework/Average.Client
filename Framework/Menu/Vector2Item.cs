using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using Newtonsoft.Json;
using System;
using static Average.Shared.SharedAPI;

namespace Average.Client.Framework.Menu
{
    internal class Vector2Input
    {
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public string PlaceHolder { get; set; }
        public object Value { get; set; }

        [JsonIgnore]
        public Func<Vector2Item, bool> Validate { get; set; }

        public Vector2Input(int minLength, int maxLength, string placeHolder, object value, Func<Vector2Item, bool> validate = null)
        {
            MinLength = minLength;
            MaxLength = maxLength;
            PlaceHolder = placeHolder;
            Value = value;
            Validate = validate;
        }
    }

    internal class Vector2Item : IPrimaryMenuItem
    {
        public string Id { get; private set; }
        public string Text { get; set; }
        public Vector2Input Primary { get; set; }
        public Vector2Input Secondary { get; set; }
        public bool Visible { get; set; }
        public bool Disabled { get; set; }

        [JsonIgnore]
        public Action<Vector2Item, object, object> OnInput { get; }

        public Vector2Item(string text, Vector2Input primary, Vector2Input secondary, Action<Vector2Item, object, object> onInput, bool visible = true, bool disabled = false)
        {
            Id = RandomString();
            Text = text;
            Primary = primary;
            Secondary = secondary;
            Visible = visible;
            Disabled = disabled;
            OnInput = onInput;
        }

        public object OnRender() => new
        {
            type = GetType().Name,
            id = Id,
            text = Text,
            primaryMinLength = Primary.MinLength,
            primaryMaxLength = Primary.MaxLength,
            primaryPlaceHolder = Primary.PlaceHolder,
            primaryValue = Primary.Value,
            secondaryMinLength = Secondary.MinLength,
            secondaryMaxLength = Secondary.MaxLength,
            secondaryPlaceHolder = Secondary.PlaceHolder,
            secondaryValue = Secondary.Value,
            visible = Visible,
            disabled = Disabled
        };

        public void OnUpdate(UIService uiService) => uiService.SendNui("menu", "render_item", OnRender());
    }
}
