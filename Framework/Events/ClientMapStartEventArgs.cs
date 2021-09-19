using System;

namespace Average.Client.Framework.Events
{
    public class ClientMapStartEventArgs : EventArgs
    {
        public string Resource { get; }

        public ClientMapStartEventArgs(string resource)
        {
            Resource = resource;
        }
    }
}
