using Average.Client.Framework.Services;
using Newtonsoft.Json;
using System;
using static Average.Shared.SharedAPI;

namespace Average.Client.Framework.Menu
{
    internal class SliderItem : IPrimaryMenuItem
    {
        public string Id { get; private set; }
        public string Text { get; set; }
        public object Min { get; set; }
        public object Max { get; set; }
        public object Step { get; set; }
        public object Value { get; set; }
        public bool Visible { get; set; }

        [JsonIgnore]
        public Action<SliderItem, object> OnInput { get; }

        public SliderItem(string text, object min, object max, object step, object value, Action<SliderItem, object> onInput, bool visible = true)
        {
            Id = RandomString();
            Text = text;
            Min = min;
            Max = max;
            Step = step;
            Value = value;
            OnInput = onInput;
            Visible = visible;
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
            visible = Visible
        };

        public void OnUpdate(UIService uiService) => uiService.SendNui("menu", "render_item", OnRender());
    }
}
