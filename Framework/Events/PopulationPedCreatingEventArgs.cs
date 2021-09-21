using CitizenFX.Core;
using System;

namespace Average.Client.Framework.Events
{
    public class PopulationPedCreatingEventArgs : EventArgs
    {
        public Vector3 Position { get; }
        public uint Model { get; }
        public dynamic OverrideCalls { get; }

        public PopulationPedCreatingEventArgs(Vector3 position, uint model, dynamic overrideCalls)
        {
            Position = position;
            Model = model;
            OverrideCalls = overrideCalls;
        }
    }
}
