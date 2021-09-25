using Average.Client.Framework.Diagnostics;
using Average.Client.Interfaces;
using System.Collections.Generic;
using static Average.Shared.SharedAPI;

namespace Average.Client.Menu
{
    internal class MenuContainer
    {
        public List<IMenuItem> Items { get; } = new();

        public string Name { get; }
        public string Title { get; }
        public string Description { get; }
        public int Index { get; set; }

        public MenuContainer(string title, string description)
        {
            Name = RandomString();
            Title = title;
            Description = description;
        }

        public bool ItemExists(IMenuItem menuItem) => Items.Exists(x => x.Name == menuItem.Name);

        public void AddItem(IMenuItem menuItem)
        {
            Logger.Error("menu item: " + menuItem.Name + ", " + ItemExists(menuItem));
            if (!ItemExists(menuItem))
            {
                menuItem.Parent = this;
                Items.Add(menuItem);
            }
        }

        public void RemoveItem(IMenuItem menuItem)
        {
            if (ItemExists(menuItem)) Items.Remove(menuItem);
        }

        public IMenuItem GetItem(string name) => Items.Find(x => x.Name == name);
    }
}
