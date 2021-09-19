using System;

namespace Average.Client.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class NetworkGetSyncAttribute : Attribute
    {
        public string Name { get; set; }

        public NetworkGetSyncAttribute(string name)
        {
            Name = name;
        }
    }
}
