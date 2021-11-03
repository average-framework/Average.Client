using CitizenFX.Core;

namespace Average.Client.Models
{
    internal class ObjectModel
    {
        public string UniqueIndex { get; set; }
        public int Handle { get; set; }
        public uint Model { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public bool IsPlacedOnGround { get; set; }
        public bool IsSpawned { get; set; }

        public ObjectModel(int handle, uint model, Vector3 position, Vector3 rotation, bool isPlacedOnGround)
        {
            Handle = handle;
            Model = model;
            Position = position;
            Rotation = rotation;
            IsPlacedOnGround = isPlacedOnGround;
        }
    }
}
