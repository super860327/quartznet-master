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
using System.Security.Cryptography;

namespace Test
{
    class Program
    {
        private static System.Text.Encoding encoding;
        /// <summary>
        /// 获取或设置加密解密的编码
        /// </summary>
        public static System.Text.Encoding Encoding
        {
            get
            {
                if (encoding == null)
                {
                    encoding = System.Text.Encoding.UTF8;
                }
                return encoding;
            }
            set
            {
                encoding = value;
            }
        }
        static void Main(string[] args)
        {

            var encryptStr = "dO+6xJH18+P37Ku0UA/etDL/K2Gxg6blCcJEsL7u+7INhz1g+AXvL5lNq1XczErYkcbiIAOuqsgS8VWs1BOrHgTMcL4gVdvjW2On1yLNQY9ahKxZ4Kld0s1VmMtCoO0lcVLzG6Uuh+00Wcd+HA8Suj3AHzJd4TboI2aNzhnoMrMQdUVHBlK0fqiIU6pmh3lEhL74XhuQAVWDWeQHLLd49SXRrAz2ijoKZoIqQRnwHRD3nWRvoHJTgnZ64AysVtbX0jyftuY2mXP8xE/UptyhaR859ZdAH9CTZWuw1Q2NwR1rxk18V9+hev8fXVMBg6/0P6LfVeqG9VvhxkHnsqchPHc1za3pf5d6Y1tmtG5TcUHhHbTVKAGfSlvWFSpgBDC9/43U6VHlvjftoGsCLw93vBf+aJ7r3JGbMGN69QOPzfgHkHETdcAAQaIjoprKhdlgCkWkbUv5Qt3jvaL7V6G6uHi6Mq7lAjgKjLyDvq5rZZ456lP+CrXp8wm/NONfBSR2M8DAxry0amOnmrJOsYYy99YiKwfr7pDhq/2ZXg53+mZl6RA4QMiFpIHwOuHYuQI+ClLcvo4MFrEdc/c+I75gSOWBJAqIwFXZtU+yZmc6NS3Hlxb+WP4wkFUQ4oMw1d2DGs2FeXMgVoMMXbIG4dGdZGVQMO+H0if+A45+nof1rMLpcdfbR51xCYg+1QZJlf8Hk1f6JaoJhSv+Plu72Cf6N1Uxky3zTcC8Jc8vI7URpSqPH7YVJMoffJO7B7C1AIsFxRUXMtKn6IEQuRHSp2gbU/domlcrAfQivUXOSwf3xwqbv6FTkyW3CGyUsfkrksOJEP/qyQak9O7NYkHaRPPimG+g3aX8PqzGb4STFIBRxTcp8+jKyw3TNst57DsSOwyOU/VQ6S9JvuBXb4y3QYw6XriBdCQxzo9zGFwmq3n/Cv0riyE5Pf8CKcLJgHMAafLg+Cm2Lv5t+CJXOs4HcgIdwN5PYVXhU13tIO4eGe7qRAaDMVpDw85xoDjkRMX8ssS7BhjkET6usmwWYmfjKJkBFRIKHEKxfN4EiHbjzoLAWls=";


            using (TripleDESCryptoServiceProvider DES = new TripleDESCryptoServiceProvider())
            {
                using (MD5CryptoServiceProvider hashMD5 = new MD5CryptoServiceProvider())
                {
                    DES.IV = Encoding.GetBytes("12345678");
                    DES.Key = Encoding.GetBytes("abcdefghijklmnopqrstuvwx");
                    DES.Mode = System.Security.Cryptography.CipherMode.CBC;
                    DES.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
                    ICryptoTransform DESDecrypt = DES.CreateDecryptor();
                    string result = "";
                    try
                    {
                        byte[] Buffer = Convert.FromBase64String(encryptStr);
                        result = Encoding.GetString(DESDecrypt.TransformFinalBlock(Buffer, 0, Buffer.Length));
                    }
                    catch (System.Exception e)
                    {
                        throw (new System.Exception("参数解密失败", e));
                    }
                   //  return result;
                }
            }


            // int i = 1234567;
            IJobDetail jobDetail = JobBuilder.Create<ColorJob>().StoreDurably().Build();
            var trigger = TriggerBuilder.Create().WithCronSchedule("0/1 * * * * ?").Build();
            int[] ints = new int[] { 1, 2, 3 };

            int d = ints.Where(i => i > 10).Sum(i => i);

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
