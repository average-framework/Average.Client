using System;

namespace Average.Client.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CommandAliasAttribute : Attribute
    {
        public string[] Alias { get; set; }

        public CommandAliasAttribute(params string[] alias)
        {
            Alias = alias;
        }
    }
}
