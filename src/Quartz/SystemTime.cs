using System;

namespace Quartz
{
    public static class SystemTime
    {
        public static Func<DateTimeOffset> UtcNow = () => DateTimeOffset.UtcNow;

        public static Func<DateTimeOffset> Now = () => DateTimeOffset.Now;
    }
}