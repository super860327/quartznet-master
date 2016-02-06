using System;
using Quartz.Util;

namespace Quartz
{
    [Serializable]
    public sealed class TriggerKey : Key<TriggerKey>
    {
        public TriggerKey(string name) : base(name, null)
        {
        }

        public TriggerKey(string name, string group) : base(name, group)
        {
        }
    }
}