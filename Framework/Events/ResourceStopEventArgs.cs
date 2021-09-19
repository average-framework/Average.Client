using System;

namespace Average.Client.Framework.Events
{
    public class ResourceStopEventArgs : EventArgs
    {
        public string Resource { get; }

        public ResourceStopEventArgs(string resource)
        {
            Resource = resource;
        }
    }
}
