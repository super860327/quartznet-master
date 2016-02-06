using System;

using Quartz.Impl.Triggers;
using Quartz.Spi;

namespace Quartz
{
    public class SimpleScheduleBuilder : ScheduleBuilder<ISimpleTrigger>
    {
        private TimeSpan interval = TimeSpan.Zero;
        private int repeatCount;
        private int misfireInstruction = MisfireInstruction.SmartPolicy;

        protected SimpleScheduleBuilder()
        {
        }

        public static SimpleScheduleBuilder Create()
        {
            return new SimpleScheduleBuilder();
        }

        public static SimpleScheduleBuilder RepeatMinutelyForever()
        {
            SimpleScheduleBuilder sb = Create()
                .WithInterval(TimeSpan.FromMinutes(1))
                .RepeatForever();

            return sb;
        }

        public static SimpleScheduleBuilder RepeatMinutelyForever(int minutes)
        {
            SimpleScheduleBuilder sb = Create()
                .WithInterval(TimeSpan.FromMinutes(minutes))
                .RepeatForever();

            return sb;
        }

        public static SimpleScheduleBuilder RepeatSecondlyForever()
        {
            SimpleScheduleBuilder sb = Create()
                .WithInterval(TimeSpan.FromSeconds(1))
                .RepeatForever();

            return sb;
        }

        public static SimpleScheduleBuilder RepeatSecondlyForever(int seconds)
        {
            SimpleScheduleBuilder sb = Create()
                .WithInterval(TimeSpan.FromSeconds(seconds))
                .RepeatForever();

            return sb;
        }

        public static SimpleScheduleBuilder RepeatHourlyForever()
        {
            SimpleScheduleBuilder sb = Create()
                .WithInterval(TimeSpan.FromHours(1))
                .RepeatForever();

            return sb;
        }

        public static SimpleScheduleBuilder RepeatHourlyForever(int hours)
        {
            SimpleScheduleBuilder sb = Create()
                .WithInterval(TimeSpan.FromHours(hours))
                .RepeatForever();

            return sb;
        }

        public static SimpleScheduleBuilder RepeatMinutelyForTotalCount(int count)
        {
            if (count < 1)
            {
                throw new ArgumentException("Total count of firings must be at least one! Given count: " + count);
            }

            SimpleScheduleBuilder sb = Create()
                .WithInterval(TimeSpan.FromMinutes(1))
                .WithRepeatCount(count - 1);

            return sb;
        }

        public static SimpleScheduleBuilder RepeatMinutelyForTotalCount(int count, int minutes)
        {
            if (count < 1)
            {
                throw new ArgumentException("Total count of firings must be at least one! Given count: " + count);
            }

            SimpleScheduleBuilder sb = Create()
                .WithInterval(TimeSpan.FromMinutes(minutes))
                .WithRepeatCount(count - 1);

            return sb;
        }

        public static SimpleScheduleBuilder RepeatSecondlyForTotalCount(int count)
        {
            if (count < 1)
            {
                throw new ArgumentException("Total count of firings must be at least one! Given count: " + count);
            }

            SimpleScheduleBuilder sb = Create()
                .WithInterval(TimeSpan.FromSeconds(1))
                .WithRepeatCount(count - 1);

            return sb;
        }

        public static SimpleScheduleBuilder RepeatSecondlyForTotalCount(int count, int seconds)
        {
            if (count < 1)
            {
                throw new ArgumentException("Total count of firings must be at least one! Given count: " + count);
            }

            SimpleScheduleBuilder sb = Create()
                .WithInterval(TimeSpan.FromSeconds(seconds))
                .WithRepeatCount(count - 1);

            return sb;
        }

        public static SimpleScheduleBuilder RepeatHourlyForTotalCount(int count)
        {
            if (count < 1)
            {
                throw new ArgumentException("Total count of firings must be at least one! Given count: " + count);
            }

            SimpleScheduleBuilder sb = Create()
                .WithInterval(TimeSpan.FromHours(1))
                .WithRepeatCount(count - 1);

            return sb;
        }

        public static SimpleScheduleBuilder RepeatHourlyForTotalCount(int count, int hours)
        {
            if (count < 1)
            {
                throw new ArgumentException("Total count of firings must be at least one! Given count: " + count);
            }

            SimpleScheduleBuilder sb = Create()
                .WithInterval(TimeSpan.FromHours(hours))
                .WithRepeatCount(count - 1);

            return sb;
        }

        public override IMutableTrigger Build()
        {
            SimpleTriggerImpl st = new SimpleTriggerImpl();
            st.RepeatInterval = interval;
            st.RepeatCount = repeatCount;
            st.MisfireInstruction = misfireInstruction;

            return st;
        }

        public SimpleScheduleBuilder WithInterval(TimeSpan timeSpan)
        {
            interval = timeSpan;
            return this;
        }

        public SimpleScheduleBuilder WithIntervalInSeconds(int seconds)
        {
            return WithInterval(TimeSpan.FromSeconds(seconds));
        }

        public SimpleScheduleBuilder WithIntervalInMinutes(int minutes)
        {
            return WithInterval(TimeSpan.FromMinutes(minutes));
        }

        public SimpleScheduleBuilder WithIntervalInHours(int hours)
        {
            return WithInterval(TimeSpan.FromHours(hours));
        }

        public SimpleScheduleBuilder WithRepeatCount(int repeatCount)
        {
            this.repeatCount = repeatCount;
            return this;
        }

        public SimpleScheduleBuilder RepeatForever()
        {
            repeatCount = SimpleTriggerImpl.RepeatIndefinitely;
            return this;
        }

        public SimpleScheduleBuilder WithMisfireHandlingInstructionIgnoreMisfires()
        {
            misfireInstruction = MisfireInstruction.IgnoreMisfirePolicy;
            return this;
        }

        public SimpleScheduleBuilder WithMisfireHandlingInstructionFireNow()
        {
            misfireInstruction = MisfireInstruction.SimpleTrigger.FireNow;
            return this;
        }

        public SimpleScheduleBuilder WithMisfireHandlingInstructionNextWithExistingCount()
        {
            misfireInstruction = MisfireInstruction.SimpleTrigger.RescheduleNextWithExistingCount;
            return this;
        }

        public SimpleScheduleBuilder WithMisfireHandlingInstructionNextWithRemainingCount()
        {
            misfireInstruction = MisfireInstruction.SimpleTrigger.RescheduleNextWithRemainingCount;
            return this;
        }

        public SimpleScheduleBuilder WithMisfireHandlingInstructionNowWithExistingCount()
        {
            misfireInstruction = MisfireInstruction.SimpleTrigger.RescheduleNowWithExistingRepeatCount;
            return this;
        }

        public SimpleScheduleBuilder WithMisfireHandlingInstructionNowWithRemainingCount()
        {
            misfireInstruction = MisfireInstruction.SimpleTrigger.RescheduleNowWithRemainingRepeatCount;
            return this;
        }

        internal SimpleScheduleBuilder WithMisfireHandlingInstruction(int readMisfireInstructionFromString)
        {
            misfireInstruction = readMisfireInstructionFromString;
            return this;
        }
    }

    public static class SimpleScheduleTriggerBuilderExtensions
    {
        public static TriggerBuilder WithSimpleSchedule(this TriggerBuilder triggerBuilder, Action<SimpleScheduleBuilder> action)
        {
            SimpleScheduleBuilder builder = SimpleScheduleBuilder.Create();
            action(builder);
            return triggerBuilder.WithSchedule(builder);
        }

        public static TriggerBuilder WithSimpleSchedule(this TriggerBuilder triggerBuilder)
        {
            SimpleScheduleBuilder builder = SimpleScheduleBuilder.Create();
            return triggerBuilder.WithSchedule(builder);
        }
    }
}