#region License

/* 
 * All content copyright Terracotta, Inc., unless otherwise indicated. All rights reserved. 
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not 
 * use this file except in compliance with the License. You may obtain a copy 
 * of the License at 
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0 
 *   
 * Unless required by applicable law or agreed to in writing, software 
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations 
 * under the License.
 * 
 */

#endregion

using System;
using System.Globalization;

using Quartz.Util;

namespace Quartz.Impl
{
    [Serializable]
    public class JobDetailImpl : IJobDetail
    {
        private string name;
        private string group = SchedulerConstants.DefaultGroup;
        private string description;
        private Type jobType;
        private JobDataMap jobDataMap;
        private bool durability;
        private bool shouldRecover;

        [NonSerialized]
        private JobKey key;

        public JobDetailImpl()
        {
        }

        public JobDetailImpl(string name, Type jobType) : this(name, null, jobType)
        {
        }

        public JobDetailImpl(string name, string group, Type jobType)
        {
            Name = name;
            Group = group;
            JobType = jobType;
        }

        public JobDetailImpl(string name, string group, Type jobType, bool isDurable, bool requestsRecovery)
        {
            Name = name;
            Group = group;
            JobType = jobType;
            Durable = isDurable;
            RequestsRecovery = requestsRecovery;
        }

        public virtual string Name
        {
            get { return name; }

            set
            {
                if (value == null || value.Trim().Length == 0)
                {
                    throw new ArgumentException("Job name cannot be empty.");
                }

                name = value;
            }
        }

        public virtual string Group
        {
            get { return group; }

            set
            {
                if (value != null && value.Trim().Length == 0)
                {
                    throw new ArgumentException("Group name cannot be empty.");
                }

                if (value == null)
                {
                    value = SchedulerConstants.DefaultGroup;
                }

                group = value;
            }
        }

        public virtual string FullName
        {
            get { return group + "." + name; }
        }

        public virtual JobKey Key
        {
            get
            {
                if (key == null)
                {
                    if (Name == null)
                    {
                        return null;
                    }
                    key = new JobKey(Name, Group);
                }

                return key;
            }
            set
            {
                Name = value != null ? value.Name : null;
                Group = value != null ? value.Group : null;
                key = value;
            }
        }

        public virtual string Description
        {
            get { return description; }
            set { description = value; }
        }

        public virtual Type JobType
        {
            get { return jobType; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentException("Job class cannot be null.");
                }

                if (!typeof(IJob).IsAssignableFrom(value))
                {
                    throw new ArgumentException("Job class must implement the Job interface.");
                }

                jobType = value;
            }
        }

        public virtual JobDataMap JobDataMap
        {
            get
            {
                if (jobDataMap == null)
                {
                    jobDataMap = new JobDataMap();
                }
                return jobDataMap;
            }

            set { jobDataMap = value; }
        }

        public virtual bool RequestsRecovery
        {
            set { shouldRecover = value; }
            get { return shouldRecover; }
        }

        public virtual bool Durable
        {
            get { return durability; }
            set { durability = value; }
        }

        public virtual bool PersistJobDataAfterExecution
        {
            get { return ObjectUtils.IsAttributePresent(jobType, typeof(PersistJobDataAfterExecutionAttribute)); }
        }

        public virtual bool ConcurrentExecutionDisallowed
        {
            get { return ObjectUtils.IsAttributePresent(jobType, typeof(DisallowConcurrentExecutionAttribute)); }
        }

        public virtual void Validate()
        {
            if (name == null)
            {
                throw new SchedulerException("Job's name cannot be null");
            }

            if (group == null)
            {
                throw new SchedulerException("Job's group cannot be null");
            }

            if (jobType == null)
            {
                throw new SchedulerException("Job's class cannot be null");
            }
        }

        public override string ToString()
        {
            return
                string.Format(
                    CultureInfo.InvariantCulture,
                    "JobDetail '{0}':  jobType: '{1} persistJobDataAfterExecution: {2} concurrentExecutionDisallowed: {3} isDurable: {4} requestsRecovers: {5}",
                    FullName, ((JobType == null) ? null : JobType.FullName), PersistJobDataAfterExecution, ConcurrentExecutionDisallowed, Durable, RequestsRecovery);
        }

        public virtual object Clone()
        {
            JobDetailImpl copy;
            try
            {
                copy = (JobDetailImpl)MemberwiseClone();
                if (jobDataMap != null)
                {
                    copy.jobDataMap = (JobDataMap)jobDataMap.Clone();
                }
            }
            catch (Exception)
            {
                throw new Exception("Not Cloneable.");
            }

            return copy;
        }

        protected virtual bool IsEqual(JobDetailImpl detail)
        {
            return (detail != null) && (detail.Name == Name) && (detail.Group == Group) &&
                   (detail.JobType == JobType);
        }

        public override bool Equals(object obj)
        {
            JobDetailImpl jd = obj as JobDetailImpl;
            if (jd == null)
            {
                return false;
            }

            return IsEqual(jd);
        }

        public virtual bool Equals(JobDetailImpl detail)
        {
            return IsEqual(detail);
        }

        public override int GetHashCode()
        {
            return FullName.GetHashCode();
        }

        public virtual JobBuilder GetJobBuilder()
        {
            JobBuilder b = JobBuilder.Create()
                .OfType(JobType)
                .RequestRecovery(RequestsRecovery)
                .StoreDurably(Durable)
                .UsingJobData(JobDataMap)
                .WithDescription(description)
                .WithIdentity(Key);

            return b;
        }
    }
}