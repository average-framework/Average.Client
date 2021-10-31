using Average.Client.Framework.Interfaces;
using System.Collections.Generic;

namespace Average.Client.Framework.Menu
{
    internal class TopContainer
    {
        public List<IPrimaryMenuItem> Items { get; } = new();

        public bool ItemExists(IPrimaryMenuItem menuItem)
        {
            return Items.Exists(x => x.Id == menuItem.Id);
        }

        public void AddItem(IPrimaryMenuItem menuItem)
        {
            if (!ItemExists(menuItem))
            {
                Items.Add(menuItem);
            }
        }

        public void RemoveItem(IPrimaryMenuItem menuItem)
        {
            if (ItemExists(menuItem))
            {
                Items.Remove(menuItem);
            }
        }

        public IEnumerable<IPrimaryMenuItem> GetItems()
        {
            return Items;
        }

        public T GetItem<T>(string id)
        {
            return (T)Items.Find(x => x.Id == id);
        }

        public object OnRender()
        {
            var items = new List<object>();

            foreach (var item in Items)
            {
                items.Add(item.OnRender());
            }

            return items;
        }
    }
}
