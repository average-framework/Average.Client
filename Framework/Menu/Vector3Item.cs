using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using Newtonsoft.Json;
using System;
using static Average.Shared.SharedAPI;

namespace Average.Client.Framework.Menu
{
    internal class Vector3Input
    {
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public string PlaceHolder { get; set; }
        public object Value { get; set; }

        [JsonIgnore]
        public Func<Vector3Item, bool> Validate { get; set; }

        public Vector3Input(int minLength, int maxLength, string placeHolder, object value, Func<Vector3Item, bool> validate = null)
        {
            MinLength = minLength;
            MaxLength = maxLength;
            PlaceHolder = placeHolder;
            Value = value;
            Validate = validate;
        }
    }

    internal class Vector3Item : IPrimaryMenuItem
    {
        public string Id { get; private set; }
        public string Text { get; set; }
        public Vector3Input Primary { get; set; }
        public Vector3Input Secondary { get; set; }
        public Vector3Input Tertiary { get; set; }
        public bool Visible { get; set; }
        public bool Disabled { get; set; }

        [JsonIgnore]
        public Action<Vector3Item, object, object, object> OnInput { get; }

        public Vector3Item(string text, Vector3Input primary, Vector3Input secondary, Vector3Input tertiary, Action<Vector3Item, object, object, object> onInput, bool visible = true, bool disabled = false)
        {
            Id = RandomString();
            Text = text;
            Primary = primary;
            Secondary = secondary;
            Tertiary = tertiary;
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
            tertiaryMinLength = Tertiary.MinLength,
            tertiaryMaxLength = Tertiary.MaxLength,
            tertiaryPlaceHolder = Tertiary.PlaceHolder,
            tertiaryValue = Tertiary.Value,
            visible = Visible,
            disabled = Disabled
        };

        public void OnUpdate(UIService uiService) => uiService.SendNui("menu", "render_item", OnRender());
    }
}
