using System;

namespace Average.Client.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ClientCommandAliasAttribute : Attribute
    {
        public string[] Alias { get; set; }

        public ClientCommandAliasAttribute(params string[] alias)
        {
            Alias = alias;
        }
    }
}
