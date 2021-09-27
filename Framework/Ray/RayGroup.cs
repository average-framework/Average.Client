using System.Collections;
using System.Collections.Generic;

namespace Average.Client.Framework.Ray
{
    internal class RayGroup : IEnumerable<RayItem>
    {
        private readonly List<RayItem> _items = new();

        public string Name { get; }
        public List<RayItem> Items => _items;

        public RayItem this[int index] => _items[index];
        public RayItem this[string rayItemId] => _items.Find(x => x.Id == rayItemId);

        public RayGroup(string groupName)
        {
            Name = groupName;
        }

        internal RayGroup AddItem(RayItem item)
        {
            _items.Add(item);
            return this;
        }

        internal RayGroup RemoveItem(RayItem item)
        {
            _items.Remove(item);
            return this;
        }

        public IEnumerator<RayItem> GetEnumerator()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                yield return _items[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
