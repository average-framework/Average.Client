using System;

namespace Average.Client.Framework.Events
{
    public class ClientResourceStartEventArgs : EventArgs
    {
        public string Resource { get; }

        public ClientResourceStartEventArgs(string resource)
        {
            Resource = resource;
        }
    }
}
