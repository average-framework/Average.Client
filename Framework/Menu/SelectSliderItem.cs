using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using Newtonsoft.Json;
using System;
using static Average.Shared.SharedAPI;

namespace Average.Client.Framework.Menu
{
    internal class SelectSliderItem : IPrimaryMenuItem
    {
        public string Id { get; private set; }
        public string Text { get; set; }
        public object Min { get; set; }
        public object Max { get; set; }
        public object Step { get; set; }
        public object Value { get; set; }
        public bool Visible { get; set; }
        public bool Disabled { get; set; }

        [JsonIgnore]
        public Type ValueType { get; set; } = typeof(int);

        [JsonIgnore]
        public Action<SelectSliderItem, SelectType, object> OnInput { get; }

        public SelectSliderItem(string text, object min, object max, object step, object value, Type valueType, Action<SelectSliderItem, SelectType, object> onInput, bool visible = true, bool disabled = false)
        {
            Id = RandomString();
            Text = text;
            Min = min;
            Max = max;
            Step = step;
            Value = value;
            ValueType = valueType;
            OnInput = onInput;
            Visible = visible;
            Disabled = disabled;
        }

        public object OnRender() => new
        {
            type = GetType().Name,
            id = Id,
            text = Text,
            min = Min,
            max = Max,
            step = Step,
            value = Value,
            visible = Visible,
            disabled = Disabled
        };

        public void OnUpdate(UIService uiService) => uiService.SendNui("menu", "render_item", OnRender());
    }
}
