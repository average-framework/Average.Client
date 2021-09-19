using System;

namespace Average.Client.Framework.Events
{
    public class ClientMapGameTypeStartEventArgs : EventArgs
    {
        public string Resource { get; }

        public ClientMapGameTypeStartEventArgs(string resource)
        {
            Resource = resource;
        }
    }
}
