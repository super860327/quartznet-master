using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Calendar;
using Quartz.Impl.Triggers;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            IJobDetail jobDetail = JobBuilder.Create<ColorJob>().StoreDurably().Build();
            var trigger = TriggerBuilder.Create().WithCronSchedule("0/1 * * * * ?").Build();

            //SimpleTriggerImpl
            //     string name, string group, 
            //     string jobName, string jobGroup, 
            //     DateTimeOffset startTimeUtc,
            //     DateTimeOffset? endTimeUtc,
            //     int repeatCount, TimeSpan repeatInterval

            //job.Durable = (true);
            //sched.AddJob(job, true);
            //log.Info("'Manually' triggering job8");
            //sched.TriggerJob("job8", "group1");

            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();

            scheduler.AddJob(jobDetail, true);
            scheduler.Start();
            scheduler.TriggerJob(jobDetail.Key);


            //ICalendar cronCalendar = new CronCalendar("0/5 * * * * ?");

            //ICalendar holidayCalendar = new HolidayCalendar();

            //sched.AddCalendar("cronCalendar", cronCalendar, true, true);

            //sched.AddCalendar("holidayCalendar", holidayCalendar, true, true);

            //JobDetail job = new JobDetail("job_" + count, schedId, typeof(SimpleRecoveryJob));

            //SimpleTrigger trigger = new SimpleTrigger("trig_" + count, schedId, 20, 5000L);

            //trigger.AddTriggerListener(new DummyTriggerListener().Name);

            //trigger.StartTime = DateTime.Now.AddMilliseconds(1000L);

            //sched.ScheduleJob(job, trigger);

            Console.Read();
        }
    }


    [PersistJobDataAfterExecutionAttribute]
    [DisallowConcurrentExecutionAttribute]
    public class ColorJob : IJob
    {
        public const string FAVORITE_COLOR = "favorite color";
        public const string EXECUTION_COUNT = "count";

        private int _counter = 1;

        public virtual void Execute(IJobExecutionContext context)
        {
            string jobName = context.JobDetail.Description;

            JobDataMap data = context.JobDetail.JobDataMap;
            string favoriteColor = data.GetString(FAVORITE_COLOR);
            int count = data.GetInt(EXECUTION_COUNT);
            count++;
            data.Put(EXECUTION_COUNT, count);
            Console.WriteLine(DateTime.Now.ToString("hh:mm:ss.fff  ") + Thread.CurrentThread.ManagedThreadId.ToString() + "  " + count.ToString());
            _counter++;
        }

    }
}
