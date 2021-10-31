using Average.Client.Framework.Interfaces;
using System;
using System.Collections.Generic;

namespace Average.Client.Framework.Menu
{
    internal class BottomContainer
    {
        public List<ISecondaryMenuItem> Items { get; } = new();

        public bool ItemExists(ISecondaryMenuItem menuItem)
        {
            if (menuItem is null)
            {
                throw new ArgumentNullException(nameof(menuItem));
            }

            return Items.Exists(x => x.Id == menuItem.Id);
        }

        public void AddItem(ISecondaryMenuItem menuItem)
        {
            if (!ItemExists(menuItem))
            {
                Items.Add(menuItem);
            }
        }

        public void RemoveItem(ISecondaryMenuItem menuItem)
        {
            if (ItemExists(menuItem))
            {
                Items.Remove(menuItem);
            }
        }

        public IEnumerable<ISecondaryMenuItem> GetItems()
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
