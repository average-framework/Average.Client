using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using System.Collections.Generic;
using static Average.Client.Framework.GameAPI;

namespace Average.Client.Framework.Menu
{
    internal class StatsMenuInfo : IMenuInfo
    {
        public string Id { get; set; }
        public bool Visible { get; set; }
        public bool Disabled { get; set; }

        public List<StatItem> Items { get; set; } = new();

        public StatsMenuInfo(bool visible = true, bool disabled = false)
        {
            Id = RandomString();
            Visible = visible;
            Disabled = Disabled;
        }

        public bool ItemExists(StatItem statsItem)
        {
            return Items.Exists(x => x.Id == statsItem.Id);
        }

        public void AddItem(StatItem statsItem)
        {
            if (!ItemExists(statsItem))
            {
                Items.Add(statsItem);
            }
        }

        public void RemoveItem(StatItem statsItem)
        {
            if (ItemExists(statsItem))
            {
                Items.Remove(statsItem);
            }
        }

        public IEnumerable<StatItem> GetItems()
        {
            return Items;
        }

        public object OnRender()
        {
            var items = new List<object>();

            foreach (var item in Items)
            {
                items.Add(item.OnRender());
            }

            return new
            {
                type = GetType().Name,
                id = Id,
                visible = Visible,
                disabled = Disabled,
                items
            };
        }

        public void OnUpdate(UIService uiService) => uiService.SendNui("menu", "render_item", OnRender());
    }
}
