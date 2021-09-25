using Average.Client.Framework.Services;
using Average.Client.Interfaces;
using System;
using static Average.Shared.SharedAPI;

namespace Average.Client.Menu
{
    internal class SelectorItem<T> : IMenuItem
    {
        public string Name { get; }
        public string Text { get; set; }
        public bool Visible { get; set; }
        public T MinValue { get; set; }
        public T MaxValue { get; set; }
        public T Value { get; set; }
        public T Step { get; set; }
        public MenuContainer Parent { get; set; }
        public Action<SelectorItem<T>> Action { get; set; }

        public SelectorItem(string text, T minValue, T maxValue, T defaultValue, T step, Action<SelectorItem<T>> action, bool visible = true)
        {
            Name = RandomString();
            Text = text;
            MinValue = minValue;
            MaxValue = maxValue;
            Value = defaultValue;
            Step = step;
            Action = action;
            Visible = visible;
        }

        public void OnRender(UIService uiService) => uiService.SendNui("menu", "render_item", new
        {
            type = "selector",
            name = Name,
            text = Text,
            //min = MinValue,
            max = MaxValue,
            value = Value,
            isFloating = Value.GetType() == typeof(float),
            //step = Step,
            visible = Visible
        });
    }
}
