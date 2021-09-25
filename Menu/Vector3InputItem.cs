using Average.Client.Framework.Services;
using Average.Client.Interfaces;
using System;
using static Average.Client.Framework.GameAPI;

namespace Average.Client.Menu
{
    internal class Vector3InputItem : IMenuItem
    {
        public string Name { get; }
        public string Text { get; set; }
        public Vector3Input Input1 { get; private set; }
        public Vector3Input Input2 { get; private set; }
        public Vector3Input Input3 { get; private set; }
        public bool Visible { get; set; }
        public MenuContainer Parent { get; set; }
        public Action<object, object, object> Action { get; private set; }

        public Vector3InputItem(string text, Vector3Input input1, Vector3Input input2, Vector3Input input3, Action<object, object, object> action, bool visible = true)
        {
            Name = RandomString();
            Text = text;
            Input1 = input1;
            Input2 = input2;
            Input3 = input3;
            Action = action;
            Visible = visible;
        }

        public void OnRender(UIService uiService) => uiService.SendNui("menu", "render_item", new
        {
            type = "vector3input",
            name = Name,

            value1 = Input1.Value,
            placeholder1 = Input1.PlaceHolder,
            pattern1 = Input1.Pattern,
            minLength1 = Input1.MinLength,
            maxLength1 = Input1.MaxLength,

            value2 = Input2.Value,
            placeholder2 = Input2.PlaceHolder,
            pattern2 = Input2.Pattern,
            minLength2 = Input2.MinLength,
            maxLength2 = Input2.MaxLength,

            value3 = Input3.Value,
            placeholder3 = Input3.PlaceHolder,
            pattern3 = Input3.Pattern,
            minLength3 = Input3.MinLength,
            maxLength3 = Input3.MaxLength,

            visible = Visible
        });
    }
}
