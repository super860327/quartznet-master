using Quartz.Spi;

namespace Quartz
{
    public abstract class ScheduleBuilder<T> : IScheduleBuilder where T : ITrigger
    {
        public abstract IMutableTrigger Build();
    }
}