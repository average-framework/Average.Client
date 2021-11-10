using System.Runtime.InteropServices;
using System.Security;

namespace Average.Client.Framework.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 8 * 12)]
    [SecurityCritical]
    internal struct PlayerCore
    {
        [FieldOffset(8 * 0)] internal long v0;
        [FieldOffset(8 * 4)] internal long v1;
        [FieldOffset(8 * 6)] internal long v2;
        [FieldOffset(8 * 8)] internal long v3;
        [FieldOffset(8 * 10)] internal long v4;

        internal PlayerCore(long v0, long v1, long v2, long v3, long v4)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
            this.v4 = v4;
        }
    }
}