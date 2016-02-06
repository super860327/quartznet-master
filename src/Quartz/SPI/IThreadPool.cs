using System.Threading;

namespace Quartz.Spi
{
    public interface IThreadPool
    {
        bool RunInThread(IThreadRunnable runnable);

        /// <summary>
        /// Determines the number of threads that are currently available in
        /// the pool.  Useful for determining the number of times
        /// <see cref="RunInThread(IThreadRunnable)"/>  can be called before returning
        /// false.
        /// </summary>
        ///<remarks>
        /// The implementation of this method should block until there is at
        /// least one available thread.
        ///</remarks>
        /// <returns>the number of currently available threads</returns>
        int BlockForAvailableThreads();

        /// <summary>
        /// Must be called before the <see cref="ThreadPool" /> is
        /// used, in order to give the it a chance to Initialize.
        /// </summary>
        /// <remarks>
        /// Typically called by the <see cref="ISchedulerFactory" />.
        /// </remarks>
        void Initialize();

        /// <summary>
        /// Called by the QuartzScheduler to inform the <see cref="ThreadPool" />
        /// that it should free up all of it's resources because the scheduler is
        /// shutting down.
        /// </summary>
        void Shutdown(bool waitForJobsToComplete);

        /// <summary>
        /// Get the current number of threads in the <see cref="IThreadPool" />.
        /// </summary>
        int PoolSize { get; }

        /// <summary>
        /// Inform the <see cref="IThreadPool" /> of the Scheduler instance's Id, 
        /// prior to initialize being invoked.
        /// </summary>
        string InstanceId { set; }

        /// <summary>
        /// Inform the <see cref="IThreadPool" /> of the Scheduler instance's name, 
        /// prior to initialize being invoked.
        /// </summary>
        string InstanceName { set; }
    }
}