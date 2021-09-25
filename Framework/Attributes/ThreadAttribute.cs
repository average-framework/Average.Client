using System;

namespace Average.Client.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ThreadAttribute : Attribute
    {
        public int StartDelay { get; } = -1;
        public int RepeatCount { get; } = 0;

        public ThreadAttribute()
        {

        }

        public ThreadAttribute(int startDelay)
        {
            StartDelay = startDelay;
        }

        public ThreadAttribute(int startDelay, int repeat)
        {
            StartDelay = startDelay;
            RepeatCount = repeat;
        }
    }
}
