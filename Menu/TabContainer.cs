using System.Collections.Generic;
using static Average.Shared.SharedAPI;

namespace Average.Client.Menu
{
    internal class TabContainer
    {
        public List<TabItem> Items { get; } = new List<TabItem>();
        public string Name { get; }

        public TabContainer()
        {
            Name = RandomString();
        }

        public bool ItemExist(TabItem menuItem) => Items.Contains(menuItem);

        public void AddItem(TabItem menuItem)
        {
            if (!ItemExist(menuItem))
            {
                Items.Add(menuItem);
            }
        }

        public void RemoveItem(TabItem menuItem)
        {
            if (ItemExist(menuItem)) Items.Remove(menuItem);
        }

        public TabItem GetItem(string name) => Items.Find(x => x.Name == name);
    }
}
