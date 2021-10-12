using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Models
{
    internal class RaycastHit
    {
        public bool Hit { get; private set; }
        public Vector3 EndCoords { get; private set; }
        public Vector3 SurfaceNormal { get; private set; }
        public int EntityHit { get; private set; }
        public int EntityType => GetEntityType(EntityHit);

        public RaycastHit(bool hit, Vector3 endCoords, Vector3 surfaceNormal, int entityHit)
        {
            Hit = hit;
            EndCoords = endCoords;
            SurfaceNormal = surfaceNormal;
            EntityHit = entityHit;
        }
    }
}
