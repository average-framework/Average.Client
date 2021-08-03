using System.Collections.Generic;
using static SDK.Client.GameAPI;

namespace Menu.Client
{
    public class MenuContainer
    {
        public List<IMenuItem> Items { get; } = new List<IMenuItem>();
        public string Name { get; }
        public string Title { get; }

        public MenuContainer(string title)
        {
            Name = RandomString();
            Title = title;
        }

        public bool ItemExists(IMenuItem menuItem) => Items.Contains(menuItem);

        public void AddItem(IMenuItem menuItem)
        {
            if (!ItemExists(menuItem))
            {
                menuItem.ParentContainer = this;
                Items.Add(menuItem);
            }
        }

        public void RemoveItem(IMenuItem menuItem)
        {
            if (ItemExists(menuItem))
                Items.Remove(menuItem);
        }

        public IMenuItem GetItem(string name) => Items.Find(x => x.Name == name);
    }
}
