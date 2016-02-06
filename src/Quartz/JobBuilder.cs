using System;

using Quartz.Impl;
using Quartz.Job;

namespace Quartz
{
    public class JobBuilder
    {
        private JobKey key;
        private string description;
        private Type jobType = typeof(NoOpJob);
        private bool durability;
        private bool shouldRecover;

        private JobDataMap jobDataMap = new JobDataMap();

        protected JobBuilder()
        {
        }

        public static JobBuilder Create()
        {
            return new JobBuilder();
        }

        public static JobBuilder Create(Type jobType)
        {
            JobBuilder b = new JobBuilder();
            b.OfType(jobType);
            return b;
        }

        public static JobBuilder Create<T>() where T : IJob
        {
            JobBuilder b = new JobBuilder();
            b.OfType(typeof(T));
            return b;
        }

        public IJobDetail Build()
        {
            JobDetailImpl job = new JobDetailImpl();

            job.JobType = jobType;
            job.Description = description;
            if (key == null)
            {
                key = new JobKey(Guid.NewGuid().ToString(), null);
            }
            job.Key = key;
            job.Durable = durability;
            job.RequestsRecovery = shouldRecover;

            if (!jobDataMap.IsEmpty)
            {
                job.JobDataMap = jobDataMap;
            }

            return job;
        }

        public JobBuilder WithIdentity(string name)
        {
            key = new JobKey(name, null);
            return this;
        }

        public JobBuilder WithIdentity(string name, string group)
        {
            key = new JobKey(name, group);
            return this;
        }

        public JobBuilder WithIdentity(JobKey key)
        {
            this.key = key;
            return this;
        }

        public JobBuilder WithDescription(string description)
        {
            this.description = description;
            return this;
        }

        public JobBuilder OfType<T>()
        {
            return OfType(typeof(T));
        }

        public JobBuilder OfType(Type type)
        {
            jobType = type;
            return this;
        }

        public JobBuilder RequestRecovery()
        {
            this.shouldRecover = true;
            return this;
        }

        public JobBuilder RequestRecovery(bool shouldRecover)
        {
            this.shouldRecover = shouldRecover;
            return this;
        }

        public JobBuilder StoreDurably()
        {
            this.durability = true;
            return this;
        }

        public JobBuilder StoreDurably(bool durability)
        {
            this.durability = durability;
            return this;
        }

        public JobBuilder UsingJobData(string key, string value)
        {
            jobDataMap.Put(key, value);
            return this;
        }

        public JobBuilder UsingJobData(string key, int value)
        {
            jobDataMap.Put(key, value);
            return this;
        }

        public JobBuilder UsingJobData(string key, long value)
        {
            jobDataMap.Put(key, value);
            return this;
        }

        public JobBuilder UsingJobData(string key, float value)
        {
            jobDataMap.Put(key, value);
            return this;
        }

        public JobBuilder UsingJobData(string key, double value)
        {
            jobDataMap.Put(key, value);
            return this;
        }

        public JobBuilder UsingJobData(string key, bool value)
        {
            jobDataMap.Put(key, value);
            return this;
        }

        public JobBuilder UsingJobData(JobDataMap newJobDataMap)
        {
            jobDataMap.PutAll(newJobDataMap);
            return this;
        }

        public JobBuilder SetJobData(JobDataMap newJobDataMap)
        {
            jobDataMap = newJobDataMap;
            return this;
        }
    }
}