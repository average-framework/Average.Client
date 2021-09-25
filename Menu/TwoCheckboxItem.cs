using Average.Client.Framework.Services;
using Average.Client.Interfaces;
using System;
using static Average.Shared.SharedAPI;

namespace Average.Client.Menu
{
    internal class TwoCheckboxItem : IMenuItem
    {
        public string Name { get; }
        public CheckboxInput Input1 { get; private set; }
        public CheckboxInput Input2 { get; private set; }
        public bool Visible { get; set; }
        public MenuContainer Parent { get; set; }
        public Action<TwoCheckboxItem> Action { get; }

        public TwoCheckboxItem(CheckboxInput input1, CheckboxInput input2, Action<TwoCheckboxItem> action, bool visible = true)
        {
            Name = RandomString();
            Input1 = input1;
            Input2 = input2;
            Action = action;
            Visible = visible;
        }

        public void OnRender(UIService uiService) => uiService.SendNui("menu", "render_item", new
        {
            type = "two_checkbox",
            name = Name,
            text1 = Input1.Text,
            text2 = Input2.Text,
            isChecked1 = Input1.IsChecked,
            isChecked2 = Input2.IsChecked,
            visible = Visible
        });
    }
}
