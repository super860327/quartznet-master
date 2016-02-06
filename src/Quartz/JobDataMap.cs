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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

using Quartz.Util;

namespace Quartz
{
    [Serializable]
    public class JobDataMap : StringKeyDirtyFlagMap
    {
        public JobDataMap() : base(15)
        {
        }

        public JobDataMap(IDictionary<string, object> map) : this()
        {
            PutAll(map);
        }

        public JobDataMap(IDictionary map) : this()
        {
            foreach (DictionaryEntry entry in map)
            {
                Put((string)entry.Key, entry.Value);
            }
        }

        protected JobDataMap(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public virtual void PutAsString(string key, bool value)
        {
            string strValue = value.ToString();
            base.Put(key, strValue);
        }


        public virtual void PutAsString(string key, char value)
        {
            string strValue = value.ToString(CultureInfo.InvariantCulture);
            base.Put(key, strValue);
        }

        public virtual void PutAsString(string key, double value)
        {
            string strValue = value.ToString(CultureInfo.InvariantCulture);
            base.Put(key, strValue);
        }

        public virtual void PutAsString(string key, float value)
        {
            string strValue = value.ToString(CultureInfo.InvariantCulture);
            base.Put(key, strValue);
        }

        public virtual void PutAsString(string key, int value)
        {
            string strValue = value.ToString(CultureInfo.InvariantCulture);
            base.Put(key, strValue);
        }

        public virtual void PutAsString(string key, long value)
        {
            string strValue = value.ToString(CultureInfo.InvariantCulture);
            base.Put(key, strValue);
        }

        public virtual void PutAsString(string key, DateTime value)
        {
            string strValue = value.ToString(CultureInfo.InvariantCulture);
            base.Put(key, strValue);
        }

        public virtual void PutAsString(string key, DateTimeOffset value)
        {
            string strValue = value.ToString(CultureInfo.InvariantCulture);
            base.Put(key, strValue);
        }

        public virtual void PutAsString(string key, TimeSpan value)
        {
            string strValue = value.ToString();
            base.Put(key, strValue);
        }

        public virtual int GetIntValueFromString(string key)
        {
            object obj = Get(key);
            return Int32.Parse((string)obj, CultureInfo.InvariantCulture);
        }

        public virtual int GetIntValue(string key)
        {
            object obj = Get(key);

            if (obj is string)
            {
                return GetIntValueFromString(key);
            }

            return GetInt(key);
        }

        public virtual bool GetBooleanValueFromString(string key)
        {
            object obj = Get(key);

            return ((string)obj).ToUpper(CultureInfo.InvariantCulture).Equals("TRUE");
        }

        public virtual bool GetBooleanValue(string key)
        {
            object obj = Get(key);

            if (obj is string)
            {
                return GetBooleanValueFromString(key);
            }

            return GetBoolean(key);
        }

        public virtual char GetCharFromString(string key)
        {
            object obj = Get(key);

            return ((string)obj)[0];
        }

        public virtual double GetDoubleValueFromString(string key)
        {
            object obj = Get(key);
            return Double.Parse((string)obj, CultureInfo.InvariantCulture);
        }

        public virtual double GetDoubleValue(string key)
        {
            object obj = Get(key);

            if (obj is string)
            {
                return GetDoubleValueFromString(key);
            }

            return GetDouble(key);
        }

        public virtual float GetFloatValueFromString(string key)
        {
            object obj = Get(key);
            return Single.Parse((string)obj, CultureInfo.InvariantCulture);
        }

        public virtual float GetFloatValue(string key)
        {
            object obj = Get(key);

            if (obj is string)
            {
                return GetFloatValueFromString(key);
            }

            return GetFloat(key);
        }

        public virtual long GetLongValueFromString(string key)
        {
            object obj = Get(key);
            return Int64.Parse((string)obj, CultureInfo.InvariantCulture);
        }

        public virtual DateTime GetDateTimeValueFromString(string key)
        {
            object obj = Get(key);
            return DateTime.Parse((string)obj, CultureInfo.InvariantCulture);
        }

        public virtual DateTimeOffset GetDateTimeOffsetValueFromString(string key)
        {
            object obj = Get(key);
            return DateTimeOffset.Parse((string)obj, CultureInfo.InvariantCulture);
        }

        public virtual TimeSpan GetTimeSpanValueFromString(string key)
        {
            object obj = Get(key);
#if NET_40
            return TimeSpan.Parse((string)obj, CultureInfo.InvariantCulture);
#else
            return TimeSpan.Parse((string) obj);
#endif
        }

        public virtual long GetLongValue(string key)
        {
            object obj = Get(key);

            if (obj is string)
            {
                return GetLongValueFromString(key);
            }

            return GetLong(key);
        }

        public virtual DateTime GetDateTimeValue(string key)
        {
            object obj = Get(key);

            if (obj is string)
            {
                return GetDateTimeValueFromString(key);
            }

            return GetDateTime(key);
        }

        public virtual DateTimeOffset GetDateTimeOffsetValue(string key)
        {
            object obj = Get(key);

            if (obj is string)
            {
                return GetDateTimeOffsetValueFromString(key);
            }

            return GetDateTimeOffset(key);
        }

        public virtual TimeSpan GetTimeSpanValue(string key)
        {
            object obj = Get(key);

            if (obj is string)
            {
                return GetTimeSpanValueFromString(key);
            }

            return GetTimeSpan(key);
        }
    }
}