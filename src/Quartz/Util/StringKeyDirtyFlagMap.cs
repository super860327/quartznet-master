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

namespace Quartz.Util
{
    [Serializable]
    public class StringKeyDirtyFlagMap : DirtyFlagMap<string, object>
    {
        public StringKeyDirtyFlagMap()
        {
        }

        public StringKeyDirtyFlagMap(int initialCapacity) : base(initialCapacity)
        {
        }

        protected StringKeyDirtyFlagMap(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override bool Equals(Object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return WrappedMap.GetHashCode();
        }

        public virtual IList<string> GetKeys()
        {
            return new List<string>(KeySet());
        }

        public override void PutAll(IDictionary<string, object> map)
        {
            foreach (KeyValuePair<string, object> pair in map)
            {
                Put(pair.Key, pair.Value);
            }
        }

        public virtual void Put(string key, int value)
        {
            base.Put(key, value);
        }

        public virtual void Put(string key, long value)
        {
            base.Put(key, value);
        }

        public virtual void Put(string key, float value)
        {
            base.Put(key, value);
        }

        public virtual void Put(string key, double value)
        {
            base.Put(key, value);
        }

        public virtual void Put(string key, bool value)
        {
            base.Put(key, value);
        }

        public virtual void Put(string key, char value)
        {
            base.Put(key, value);
        }

        public virtual void Put(string key, string value)
        {
            base.Put(key, value);
        }

        public virtual int GetInt(string key)
        {
            object obj = this[key];

            try
            {
                return Convert.ToInt32(obj);
            }
            catch (Exception)
            {
                throw new InvalidCastException("Identified object is not an Integer.");
            }
        }

        public virtual long GetLong(string key)
        {
            object obj = this[key];

            try
            {
                return Convert.ToInt64(obj);
            }
            catch (Exception)
            {
                throw new InvalidCastException("Identified object is not a Long.");
            }
        }

        public virtual float GetFloat(string key)
        {
            object obj = this[key];

            try
            {
                return Convert.ToSingle(obj);
            }
            catch (Exception)
            {
                throw new InvalidCastException("Identified object is not a Float.");
            }
        }

        public virtual double GetDouble(string key)
        {
            object obj = this[key];

            try
            {
                return Convert.ToDouble(obj);
            }
            catch (Exception)
            {
                throw new InvalidCastException("Identified object is not a Double.");
            }
        }

        public virtual bool GetBoolean(string key)
        {
            object obj = this[key];

            try
            {
                return Convert.ToBoolean(obj);
            }
            catch (Exception)
            {
                throw new InvalidCastException("Identified object is not a Boolean.");
            }
        }

        public virtual char GetChar(string key)
        {
            object obj = this[key];

            try
            {
                return Convert.ToChar(obj);
            }
            catch (Exception)
            {
                throw new InvalidCastException("Identified object is not a Character.");
            }
        }

        public virtual string GetString(string key)
        {
            object obj = this[key];

            try
            {
                return (string) obj;
            }
            catch (Exception)
            {
                throw new InvalidCastException("Identified object is not a String.");
            }
        }

        public virtual DateTime GetDateTime(string key)
        {
            object obj = this[key];

            try
            {
                return Convert.ToDateTime(obj, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                throw new InvalidCastException("Identified object is not a DateTime.");
            }
        }

        public virtual DateTimeOffset GetDateTimeOffset(string key)
        {
            object obj = this[key];
            return (DateTimeOffset) obj;
        }

        public virtual TimeSpan GetTimeSpan(string key)
        {
            object obj = this[key];
            return (TimeSpan) obj;
        }
    }
}