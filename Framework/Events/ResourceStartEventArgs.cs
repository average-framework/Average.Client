using System;

namespace Average.Client.Framework.Events
{
    public class ResourceStartEventArgs : EventArgs
    {
        public string Resource { get; }

        public ResourceStartEventArgs(string resource)
        {
            Resource = resource;
        }
    }
}
