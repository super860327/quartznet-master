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

using Quartz.Spi;

namespace Quartz
{
    public interface ITrigger : ICloneable, IComparable<ITrigger>
    {
        TriggerKey Key { get; }

        JobKey JobKey { get; }

        TriggerBuilder GetTriggerBuilder();

        IScheduleBuilder GetScheduleBuilder();

        string Description { get; }

        string CalendarName { get; }

        JobDataMap JobDataMap { get; }

        DateTimeOffset? FinalFireTimeUtc { get; }

        int MisfireInstruction { get; }

        DateTimeOffset? EndTimeUtc { get; }

        DateTimeOffset StartTimeUtc { get; }

        int Priority { get; set; }

        bool GetMayFireAgain();

        DateTimeOffset? GetNextFireTimeUtc();

        DateTimeOffset? GetPreviousFireTimeUtc();

        DateTimeOffset? GetFireTimeAfter(DateTimeOffset? afterTime);

        bool HasMillisecondPrecision { get; }
    }
}