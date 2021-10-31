using Average.Client.Framework.Services;
using Newtonsoft.Json;
using System;
using static Average.Shared.SharedAPI;

namespace Average.Client.Framework.Menu
{
    internal class Vector3
    {
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public string PlaceHolder { get; set; }
        public object Value { get; set; }

        public Vector3(int minLength, int maxLength, string placeHolder, object value)
        {
            MinLength = minLength;
            MaxLength = maxLength;
            PlaceHolder = placeHolder;
            Value = value;
        }
    }

    internal class Vector3Item : IPrimaryMenuItem
    {
        public string Id { get; private set; }
        public string Text { get; set; }
        public Vector3 Primary { get; set; }
        public Vector3 Secondary { get; set; }
        public Vector3 Tertiary { get; set; }
        public bool Visible { get; set; }

        [JsonIgnore]
        public Action<Vector3Item, object, object, object> OnInput { get; }

        public Vector3Item(string text, Vector3 primary, Vector3 secondary, Vector3 tertiary, Action<Vector3Item, object, object, object> onInput, bool visible = true)
        {
            Id = RandomString();
            Text = text;
            Primary = primary;
            Secondary = secondary;
            Tertiary = tertiary;
            Visible = visible;
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
            visible = Visible
        };

        public void OnUpdate(UIService uiService) => uiService.SendNui("menu", "render_item", OnRender());
    }
}
