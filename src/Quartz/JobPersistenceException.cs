using System;
using System.Runtime.Serialization;

namespace Quartz
{
    [Serializable]
	public class JobPersistenceException : SchedulerException
	{
		public JobPersistenceException(string msg) : base(msg)
		{
		}

        protected JobPersistenceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

		public JobPersistenceException(string msg, Exception cause) : base(msg, cause)
		{
		}
	}
}
