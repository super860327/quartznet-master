using System;

namespace Quartz.Spi
{
    public interface IMutableTrigger : ITrigger
    {
        new TriggerKey Key { set; get; }

        new JobKey JobKey { set; get; }

        new string Description { get; set; }

        new string CalendarName { set; get; }

        new JobDataMap JobDataMap { get; set; }

        new int Priority { get; set; }

        new DateTimeOffset StartTimeUtc { get; set; }

        new DateTimeOffset? EndTimeUtc { get; set; }

        new int MisfireInstruction { get; set; }
    }
}