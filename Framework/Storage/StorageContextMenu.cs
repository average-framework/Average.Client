using System.Collections.Generic;
using System.Linq;

namespace Average.Client.Framework.Storage
{
    internal class StorageContextMenu
    {
        public List<StorageContextItem> Items { get; } = new List<StorageContextItem>();

        public StorageContextMenu(params StorageContextItem[] items) => Items = items.ToList();

        public void AddContext(StorageContextItem item) => Items.Add(item);
        public void RemoveContext(StorageContextItem item) => Items.Remove(item);
        public StorageContextItem GetContext(string eventName) => Items.Find(x => x.EventName == eventName);
    }
}