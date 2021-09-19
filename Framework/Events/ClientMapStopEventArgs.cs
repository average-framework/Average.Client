using System;

namespace Average.Client.Framework.Events
{
    public class ClientMapStopEventArgs : EventArgs
    {
        public string Resource { get; }

        public ClientMapStopEventArgs(string resource)
        {
            Resource = resource;
        }
    }
}
