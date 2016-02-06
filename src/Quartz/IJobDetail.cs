using System;

namespace Quartz
{
    public interface IJobDetail : ICloneable
    {
        JobKey Key { get; }

        string Description { get; }

        Type JobType { get; }

        JobDataMap JobDataMap { get; }

        bool Durable { get; }

        bool PersistJobDataAfterExecution { get; }

        bool ConcurrentExecutionDisallowed { get; }

        bool RequestsRecovery { get; }

        JobBuilder GetJobBuilder();
    }
}