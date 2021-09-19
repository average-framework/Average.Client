using System;

namespace Average.Client.Framework.Extensions
{
    public static class FloatExtentions
    {
        public static bool IsBetween(this float value, float min, float max) => value > min && value < max;
        public static float DegreesToRadians(this float val) => (float)(Math.PI / 180 * val);
        public static double RadiansToDegrees(this float val) => 180 / Math.PI * val;
        public static float Lerp(this float val, float min, float max) => (1 - val) * min + val * max;
    }
}
