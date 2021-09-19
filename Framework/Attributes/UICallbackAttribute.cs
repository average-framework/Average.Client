using System;

namespace Average.Client.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class UICallbackAttribute : Attribute
    {
        public string Name { get; }

        public UICallbackAttribute(string name)
        {
            Name = name;
        }
    }
}
