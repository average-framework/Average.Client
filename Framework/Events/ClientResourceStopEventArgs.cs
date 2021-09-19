using System;

namespace Average.Client.Framework.Events
{
    public class ClientResourceStopEventArgs : EventArgs
    {
        public string Resource { get; }

        public ClientResourceStopEventArgs(string resource)
        {
            Resource = resource;
        }
    }
}
