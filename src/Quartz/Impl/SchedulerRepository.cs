using System.Collections.Generic;
using System.Globalization;

namespace Quartz.Impl
{
	public class SchedulerRepository
	{
        private readonly Dictionary<string, IScheduler> schedulers;
        private static readonly SchedulerRepository inst = new SchedulerRepository();
        private readonly object syncRoot = new object();
        
        /// <summary>
		/// Gets the singleton instance.
		/// </summary>
		/// <value>The instance.</value>
		public static SchedulerRepository Instance
		{
			get { return inst; }
		}

		private SchedulerRepository()
		{
			schedulers = new Dictionary<string, IScheduler>();
		}

        /// <summary>
        /// Binds the specified sched.
        /// </summary>
        /// <param name="sched">The sched.</param>
		public virtual void Bind(IScheduler sched)
		{
			lock (syncRoot)
			{
				if (schedulers.ContainsKey(sched.SchedulerName))
				{
					throw new SchedulerException(string.Format(CultureInfo.InvariantCulture, "Scheduler with name '{0}' already exists.", sched.SchedulerName));
				}

				schedulers[sched.SchedulerName] = sched;
			}
		}

        /// <summary>
        /// Removes the specified sched name.
        /// </summary>
        /// <param name="schedName">Name of the sched.</param>
        /// <returns></returns>
		public virtual bool Remove(string schedName)
		{
			lock (syncRoot)
			{
				return schedulers.Remove(schedName);
			}
		}

        /// <summary>
        /// Lookups the specified sched name.
        /// </summary>
        /// <param name="schedName">Name of the sched.</param>
        /// <returns></returns>
		public virtual IScheduler Lookup(string schedName)
		{
			lock (syncRoot)
			{
			    IScheduler retValue;
			    schedulers.TryGetValue(schedName, out retValue);
				return retValue;
			}
		}

        /// <summary>
        /// Lookups all.
        /// </summary>
        /// <returns></returns>
		public virtual ICollection<IScheduler> LookupAll()
		{
			lock (syncRoot)
			{
				return new List<IScheduler>(schedulers.Values).AsReadOnly();
			}
		}
	}
}
