using System;

namespace Average.Client.Framework.Events
{
    public class ResourceStartingEventArgs : EventArgs
    {
        public string Resource { get; }

        public ResourceStartingEventArgs(string resource)
        {
            Resource = resource;
        }
    }
}
