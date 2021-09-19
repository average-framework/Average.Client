using System;

namespace Average.Client.Framework.Events
{
    public class ClientMapGameTypeStopEventArgs : EventArgs
    {
        public string Resource { get; }

        public ClientMapGameTypeStopEventArgs(string resource)
        {
            Resource = resource;
        }
    }
}
