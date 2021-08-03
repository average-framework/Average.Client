using SDK.Client.Menu;
using System.Collections.Generic;
using static SDK.Client.GameAPI;

namespace Average.Client.Menu
{
    public class MenuContainer : IMenuContainer
    {
        public List<IMenuItem> Items { get; } = new List<IMenuItem>();
        public string Name { get; }
        public string Title { get; }

        public MenuContainer(string title)
        {
            Name = RandomString();
            Title = title;
        }

        public bool ItemExist(IMenuItem menuItem) => Items.Contains(menuItem);

        public void AddItem(IMenuItem menuItem)
        {
            if (!ItemExist(menuItem))
            {
                menuItem.ParentContainer = this;
                Items.Add(menuItem);
            }
        }

        public void RemoveItem(IMenuItem menuItem)
        {
            if (ItemExist(menuItem)) Items.Remove(menuItem);
        }

        public IMenuItem GetItem(string name) => Items.Find(x => x.Name == name);
    }
}
