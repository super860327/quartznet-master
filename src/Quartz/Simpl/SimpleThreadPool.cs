using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

using Common.Logging;

using Quartz.Spi;

namespace Quartz.Simpl
{
    public class SimpleThreadPool : IThreadPool
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SimpleThreadPool));
        private const int DefaultThreadPoolSize = 10;

        private readonly object nextRunnableLock = new object();
        private readonly LinkedList<WorkerThread> availWorkers = new LinkedList<WorkerThread>();
        private readonly LinkedList<WorkerThread> busyWorkers = new LinkedList<WorkerThread>();

        private int count = DefaultThreadPoolSize;
        private bool handoffPending;
        private bool isShutdown;
        private ThreadPriority prio = ThreadPriority.Normal;
        private string schedulerInstanceName;

        private List<WorkerThread> workers;

        public SimpleThreadPool()
        {
        }

        public SimpleThreadPool(int threadCount, ThreadPriority threadPriority)
        {
            ThreadCount = threadCount;
            ThreadPriority = threadPriority;
        }

        public int ThreadCount
        {
            get { return count; }
            set { count = value; }
        }

        public ThreadPriority ThreadPriority
        {
            get { return prio; }
            set { prio = value; }
        }

        public virtual string ThreadNamePrefix { get; set; }

        public virtual bool MakeThreadsDaemons { get; set; }

        public virtual int PoolSize
        {
            get { return ThreadCount; }
        }

        public virtual string InstanceId
        {
            set { }
        }

        public virtual string InstanceName
        {
            set { schedulerInstanceName = value; }
        }

        public virtual void Initialize()
        {
            if (workers != null && workers.Count > 0) return;

            if (count <= 0) throw new SchedulerConfigException("Thread count must be > 0");

            foreach (WorkerThread wt in CreateWorkerThreads(count))
            {
                wt.Start();
                availWorkers.AddLast(wt);
            }
        }

        public virtual void Shutdown(bool waitForJobsToComplete)
        {
            lock (nextRunnableLock)
            {
                isShutdown = true;

                if (workers == null) return;

                foreach (WorkerThread thread in workers)
                {
                    if (thread != null)
                    {
                        thread.Shutdown();
                    }
                }
                Monitor.PulseAll(nextRunnableLock);

                if (waitForJobsToComplete)
                {
                    bool interrupted = false;
                    try
                    {
                        while (handoffPending)
                        {
                            try
                            {
                                Monitor.Wait(nextRunnableLock, 100);
                            }
                            catch (ThreadInterruptedException)
                            {
                                interrupted = true;
                            }
                        }

                        while (busyWorkers.Count > 0)
                        {
                            LinkedListNode<WorkerThread> wt = busyWorkers.First;
                            try
                            {
                                log.DebugFormat(CultureInfo.InvariantCulture, "Waiting for thread {0} to shut down", wt.Value.Name);

                                Monitor.Wait(nextRunnableLock, 2000);
                            }
                            catch (ThreadInterruptedException)
                            {
                                interrupted = true;
                            }
                        }

                        while (workers.Count > 0)
                        {
                            int index = workers.Count - 1;
                            WorkerThread wt = workers[index];
                            try
                            {
                                wt.Join();
                                workers.RemoveAt(index);
                            }
                            catch (ThreadInterruptedException)
                            {
                                interrupted = true;
                            }
                        }
                    }
                    finally
                    {
                        if (interrupted)
                        {
                            Thread.CurrentThread.Interrupt();
                        }
                    }

                    log.Debug("No executing jobs remaining, all threads stopped.");
                }

                log.Debug("Shutdown of threadpool complete.");
            }
        }

        public virtual bool RunInThread(IThreadRunnable runnable)
        {
            if (runnable == null) return false;

            lock (nextRunnableLock)
            {
                handoffPending = true;

                while ((availWorkers.Count < 1) && !isShutdown)
                {
                    try
                    {
                        Monitor.Wait(nextRunnableLock, 500);
                    }
                    catch (ThreadInterruptedException)
                    {
                    }
                }

                if (!isShutdown)
                {
                    WorkerThread wt = availWorkers.First.Value;
                    availWorkers.RemoveFirst();
                    busyWorkers.AddLast(wt);
                    wt.Run(runnable);
                }
                else
                {
                    WorkerThread wt = new WorkerThread(this, "WorkerThread-LastJob", prio, MakeThreadsDaemons, runnable);
                    busyWorkers.AddLast(wt);
                    workers.Add(wt);
                    wt.Start();
                }
                Monitor.PulseAll(nextRunnableLock);
                handoffPending = false;
            }

            return true;
        }

        public int BlockForAvailableThreads()
        {
            lock (nextRunnableLock)
            {
                while ((availWorkers.Count < 1 || handoffPending) && !isShutdown)
                {
                    try
                    {
                        Monitor.Wait(nextRunnableLock, 500);
                    }
                    catch (ThreadInterruptedException)
                    {
                    }
                }

                return availWorkers.Count;
            }
        }

        protected void MakeAvailable(WorkerThread wt)
        {
            lock (nextRunnableLock)
            {
                if (!isShutdown)
                {
                    availWorkers.AddLast(wt);
                }
                busyWorkers.Remove(wt);
                Monitor.PulseAll(nextRunnableLock);
            }
        }

        protected virtual IList<WorkerThread> CreateWorkerThreads(int threadCount)
        {
            workers = new List<WorkerThread>();
            for (int i = 1; i <= threadCount; ++i)
            {
                string threadPrefix = ThreadNamePrefix;
                if (threadPrefix == null)
                {
                    threadPrefix = schedulerInstanceName + "_Worker";
                }

                var workerThread = new WorkerThread(
                    this,
                    string.Format(CultureInfo.InvariantCulture, "{0}-{1}", threadPrefix, i),
                    ThreadPriority,
                    MakeThreadsDaemons);

                workers.Add(workerThread);
            }

            return workers;
        }

        public virtual void Shutdown()
        {
            Shutdown(true);
        }

        protected virtual void ClearFromBusyWorkersList(WorkerThread wt)
        {
            lock (nextRunnableLock)
            {
                busyWorkers.Remove(wt);
                Monitor.PulseAll(nextRunnableLock);
            }
        }

        protected class WorkerThread : QuartzThread
        {
            private readonly object lockObject = new object();

            private volatile bool canRun = true;

            private IThreadRunnable runnable;
            private readonly SimpleThreadPool tp;
            private readonly bool runOnce;

            internal WorkerThread(SimpleThreadPool tp, string name,
                                  ThreadPriority prio, bool isDaemon)
                : this(tp, name, prio, isDaemon, null)
            {
            }

            internal WorkerThread(SimpleThreadPool tp, string name,
                                  ThreadPriority prio, bool isDaemon, IThreadRunnable runnable)
                : base(name)
            {
                this.tp = tp;
                this.runnable = runnable;
                if (runnable != null)
                {
                    runOnce = true;
                }
                Priority = prio;
                IsBackground = isDaemon;
            }

            internal virtual void Shutdown()
            {
                canRun = false;
            }

            public void Run(IThreadRunnable newRunnable)
            {
                lock (lockObject)
                {
                    if (runnable != null)
                    {
                        throw new ArgumentException("Already running a Runnable!");
                    }

                    runnable = newRunnable;
                    Monitor.PulseAll(lockObject);
                }
            }

            public override void Run()
            {
                bool running = false;

                while (canRun)
                {
                    try
                    {
                        lock (lockObject)
                        {
                            while (runnable == null && canRun)
                            {
                                Monitor.Wait(lockObject, 500);
                            }
                            if (runnable != null)
                            {
                                running = true;
                                runnable.Run();
                            }

                        }
                    }
                    catch (Exception exceptionInRunnable)
                    {
                        log.Error("Error while executing the Runnable: ", exceptionInRunnable);
                    }
                    finally
                    {
                        lock (lockObject)
                        {
                            runnable = null;
                        }
                        // repair the thread in case the runnable mucked it up...
                        Priority = tp.ThreadPriority;

                        if (runOnce)
                        {
                            canRun = false;
                            tp.ClearFromBusyWorkersList(this);
                        }
                        else if (running)
                        {
                            running = false;
                            tp.MakeAvailable(this);
                        }
                    }
                }

                log.Debug("WorkerThread is shut down");
            }
        }
    }
}
