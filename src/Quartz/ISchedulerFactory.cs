using System.Collections.Generic;

namespace Quartz
{
    public interface ISchedulerFactory
    {
        ICollection<IScheduler> AllSchedulers { get; }

        IScheduler GetScheduler();

        IScheduler GetScheduler(string schedName);
    }
}