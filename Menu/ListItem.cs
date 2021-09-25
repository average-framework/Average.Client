using Average.Client.Framework.Services;
using Average.Client.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using static Average.Shared.SharedAPI;

namespace Average.Client.Menu
{
    internal class ListItem : IMenuItem
    {
        public string Name { get; }
        public string Text { get; set; }
        public int Index { get; set; }
        public bool Visible { get; set; }
        public bool ShowValue { get; set; }
        public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();
        public MenuContainer Parent { get; set; }
        public Action<int, Dictionary<string, object>> Action { get; }

        public ListItem(string text, int index, Dictionary<string, object> values, Action<int, Dictionary<string, object>> action, bool visible = true, bool showValue = false)
        {
            Name = RandomString();
            Text = text;
            Values = values;
            Index = index;
            Action = action;
            Visible = visible;
            ShowValue = showValue;
        }

        public void OnRender(UIService uiService) => uiService.SendNui("menu", "render_item", new
        {
            type = "list",
            name = Name,
            text = Text,
            itemName = ShowValue ? Values.ElementAt(Index).Value : Values.ElementAt(Index).Key,
            visible = Visible
        });
    }
}
