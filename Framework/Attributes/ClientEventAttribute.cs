using System;

namespace Average.Client.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ClientEventAttribute : Attribute
    {
        public string Event { get; }

        public ClientEventAttribute(string @event)
        {
            Event = @event;
        }
    }
}
