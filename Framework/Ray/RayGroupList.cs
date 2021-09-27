using System;
using System.Collections;
using System.Collections.Generic;
using static Average.Client.Framework.Services.RayService;

namespace Average.Client.Framework.Ray
{
    internal class RayGroupList : IList<RayGroup>
    {
        private readonly List<RayGroup> _groups = new();

        public int Count => _groups.Count;
        public bool IsReadOnly => true;

        RayGroup IList<RayGroup>.this[int index] { get => _groups[index]; set => _groups[index] = value; }

        public RayGroup this[int index] => _groups[index];
        public RayGroup this[string groupName] => _groups.Find(x => x.Name == groupName);

        internal void RemoveGroup(string groupName)
        {
            if (GroupExists(groupName))
            {
                _groups.Remove(GetGroup(groupName));
            }
        }

        internal void RemoveGroup(RayGroup group)
        {
            if (GroupExists(group.Name))
            {
                _groups.Remove(group);
            }
        }

        internal bool GroupExists(RayGroup group)
        {
            return this[group.Name] != null;
        }

        internal bool GroupExists(string groupName)
        {
            return this[groupName] != null;
        }

        internal RayGroup GetGroup(string groupName)
        {
            if (GroupExists(groupName))
            {
                return this[groupName];
            }

            throw new Exception($"[RayService] Unable to get group: {groupName}.");
        }

        public IEnumerator<RayGroup> GetEnumerator()
        {
            for (int i = 0; i < _groups.Count; i++)
            {
                yield return _groups[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(RayGroup item)
        {
            return _groups.IndexOf(item);
        }

        public void Insert(int index, RayGroup item)
        {
            _groups.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _groups.RemoveAt(index);
        }

        public void Add(RayGroup item)
        {
            _groups.Add(item);
        }

        public void Clear()
        {
            _groups.Clear();
        }

        public bool Contains(RayGroup item)
        {
            return _groups.Contains(item);
        }

        public void CopyTo(RayGroup[] array, int arrayIndex)
        {
            _groups.CopyTo(array, arrayIndex);
        }

        public bool Remove(RayGroup item)
        {
            return _groups.Remove(item);
        }
    }
}
