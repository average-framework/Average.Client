using Average.Client.Framework.Services;
using Average.Client.Interfaces;
using static Average.Shared.SharedAPI;

namespace Average.Client.Menu
{
    internal class BarItem : IMenuItem
    {
        public string Name { get; }
        public string Text { get; set; }
        public string Description { get; set; }
        public int Step { get; set; }
        public int Value { get; set; }
        public bool Visible { get; set; }
        public MenuContainer Parent { get; set; }

        public BarItem(string text, string description, int step, int value, bool visible = true)
        {
            Name = RandomString();
            Text = text;
            Description = description;
            Step = step;
            Value = value;
            Visible = visible;
        }

        public void OnRender(UIService uiService) => uiService.SendNui("menu", "render_item", new
        {
            type = "bar",
            name = Name,
            text = Text,
            description = Description,
            step = Step,
            value = Value,
            visible = Visible
        });
    }
}
