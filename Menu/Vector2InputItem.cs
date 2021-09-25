using Average.Client.Framework.Services;
using Average.Client.Interfaces;
using System;
using static Average.Shared.SharedAPI;

namespace Average.Client.Menu
{
    internal class Vector2InputItem : IMenuItem
    {
        public string Name { get; }
        public Vector2Input Input1 { get; private set; }
        public Vector2Input Input2 { get; private set; }
        public bool Visible { get; set; }
        public MenuContainer Parent { get; set; }
        public Action<object, object> Action { get; private set; }

        public Vector2InputItem(Vector2Input input1, Vector2Input input2, Action<object, object> action, bool visible = true)
        {
            Name = RandomString();
            Input1 = input1;
            Input2 = input2;
            Action = action;
            Visible = visible;
        }

        public void OnRender(UIService uiService) => uiService.SendNui("menu", "render_item", new
        {
            type = "vector2input",
            name = Name,
            text1 = Input1.Text,
            value1 = Input1.Value,
            placeholder1 = Input1.PlaceHolder,
            pattern1 = Input1.Pattern,
            minLength1 = Input1.MinLength,
            maxLength1 = Input1.MaxLength,
            text2 = Input2.Text,
            value2 = Input2.Value,
            placeholder2 = Input2.PlaceHolder,
            pattern2 = Input2.Pattern,
            minLength2 = Input2.MinLength,
            maxLength2 = Input2.MaxLength,
            visible = Visible
        });
    }
}
