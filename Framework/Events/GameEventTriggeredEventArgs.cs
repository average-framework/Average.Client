using System;

namespace Average.Client.Framework.Events
{
    public class GameEventTriggeredEventArgs : EventArgs
    {
        public string Name { get; }
        public int[] Data { get; }

        public GameEventTriggeredEventArgs(string name, int[] data)
        {
            Name = name;
            Data = data;
        }
    }
}
