using System;

namespace Average.Client.IoC
{
    internal class ContainerItem
    {
        public string ServiceKey { get; private set; }
        public Type Type { get; private set; }
        public Reuse Reuse { get; private set; }
        public object Instance { get; set; }

        public ContainerItem(Type type, Reuse reuse, string serviceKey = "", object instance = null)
        {
            Type = type;
            Reuse = reuse;
            ServiceKey = serviceKey;
            Instance = instance;
        }
    }
}
