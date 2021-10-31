using System;

namespace Average.Client.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ClientCommandAttribute : Attribute
    {
        public string Command { get; }
        public int PermissionLevel { get; }

        public ClientCommandAttribute(string command, int permissionLevel = 0)
        {
            Command = command;
            PermissionLevel = permissionLevel;
        }
    }
}
