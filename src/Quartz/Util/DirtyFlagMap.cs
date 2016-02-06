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
using System.Runtime.Serialization;
using System.Security;

namespace Quartz.Util
{
    [Serializable]
    public class DirtyFlagMap<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, ICloneable, ISerializable
    {
        private bool dirty;
        private Dictionary<TKey, TValue> map;
        private readonly object syncRoot = new object();

        public DirtyFlagMap()
        {
            map = new Dictionary<TKey, TValue>();
        }

        public DirtyFlagMap(int initialCapacity)
        {
            map = new Dictionary<TKey, TValue>(initialCapacity);
        }

        /// <summary>
        /// Serialization constructor.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected DirtyFlagMap(SerializationInfo info, StreamingContext context)
        {
            int version;
            try
            {
                version = info.GetInt32("version");
            }
            catch
            {
                version = 0;
            }


            string prefix = "";
            if (version < 1)
            {
                try
                {
                    info.GetValue("dirty", typeof(bool));
                }
                catch
                {
                    // base class qualified format
                    prefix = "DirtyFlagMap+";
                }
            }

            switch (version)
            {
                case 0:
                    object o = info.GetValue(prefix + "map", typeof(object));
                    Hashtable oldMap = o as Hashtable;
                    if (oldMap != null)
                    {
                        // need to call ondeserialization to get hashtable
                        // initialized correctly
                        oldMap.OnDeserialization(this);

                        map = new Dictionary<TKey, TValue>();
                        foreach (DictionaryEntry entry in oldMap)
                        {
                            map.Add((TKey)entry.Key, (TValue)entry.Value);
                        }
                    }
                    else
                    {
                        // new version
                        map = (Dictionary<TKey, TValue>)o;
                    }
                    break;
                case 1:
                    dirty = (bool)info.GetValue("dirty", typeof(bool));
                    map = (Dictionary<TKey, TValue>)info.GetValue("map", typeof(Dictionary<TKey, TValue>));
                    break;
                default:
                    throw new NotSupportedException("Unknown serialization version");
            }

        }

        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);
            info.AddValue("dirty", dirty);
            info.AddValue("map", map);
        }

        /// <summary>
        /// Determine whether the <see cref="IDictionary" /> is flagged dirty.
        /// </summary>
        public virtual bool Dirty
        {
            get { return dirty; }
        }

        public virtual IDictionary<TKey, TValue> WrappedMap
        {
            get { return map; }
        }

        public virtual bool IsEmpty
        {
            get { return (map.Count == 0); }
        }

        #region ICloneable Members

        public virtual object Clone()
        {
            DirtyFlagMap<TKey, TValue> copy;
            try
            {
                copy = (DirtyFlagMap<TKey, TValue>)MemberwiseClone();
                copy.map = new Dictionary<TKey, TValue>(map);
            }
            catch (Exception)
            {
                throw new Exception("Not Cloneable.");
            }

            return copy;
        }

        #endregion

        #region IDictionary Members

        public bool TryGetValue(TKey key, out TValue value)
        {
            return map.TryGetValue(key, out value);
        }

        public virtual TValue Get(TKey key)
        {
            return this[key];
        }

        public virtual TValue this[TKey key]
        {
            get
            {
                TValue temp;
                map.TryGetValue(key, out temp);
                return temp;
            }
            set
            {
                map[key] = value;
                dirty = true;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        public virtual int Count
        {
            get { return map.Count; }
        }

        ICollection IDictionary.Keys
        {
            get { return map.Keys; }
        }

        ICollection IDictionary.Values
        {
            get { return map.Values; }
        }

        public virtual ICollection<TValue> Values
        {
            get { return map.Values; }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Put(item.Key, item.Value);
        }

        public bool Contains(object key)
        {
            return ((IDictionary)map).Contains(key);
        }

        public void Add(object key, object value)
        {
            Put((TKey)key, (TValue)value);
        }

        public virtual void Clear()
        {
            if (map.Count != 0)
            {
                dirty = true;
            }

            map.Clear();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IDictionary)map).GetEnumerator();
        }

        public void Remove(object key)
        {
            Remove((TKey)key);
        }

        object IDictionary.this[object key]
        {
            get { return this[(TKey)key]; }
            set { this[(TKey)key] = (TValue)value; }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return Contains(item.Key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)map).CopyTo(array, arrayIndex);
        }

        public virtual bool ContainsKey(TKey key)
        {
            return map.ContainsKey(key);
        }

        public virtual bool Remove(TKey key)
        {
            bool remove = map.Remove(key);
            dirty |= remove;
            return remove;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return map.GetEnumerator();
        }

        public virtual void Add(TKey key, TValue value)
        {
            map.Add(key, value);
            dirty = true;
        }

        public virtual void CopyTo(Array array, int index)
        {
            TKey[] keys = new TKey[Count];
            TValue[] values = new TValue[Count];

            Keys.CopyTo(keys, index);
            Values.CopyTo(values, index);

            for (int i = index; i < Count; i++)
            {
                if (!Equals(keys[i], default(TKey)) || !Equals(values[i], default(TValue)))
                {
                    array.SetValue(new DictionaryEntry(keys[i], values[i]), i);
                }
            }
        }

        public virtual ICollection<TKey> Keys
        {
            get { return map.Keys; }
        }

        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        public virtual bool IsFixedSize
        {
            get { return false; }
        }

        public virtual object SyncRoot
        {
            get { return syncRoot; }
        }

        public virtual bool IsSynchronized
        {
            get { return false; }
        }

        #endregion

        public virtual void ClearDirtyFlag()
        {
            dirty = false;
        }

        public virtual bool ContainsValue(TValue obj)
        {
            return map.ContainsValue(obj);
        }

        public virtual Dictionary<TKey, TValue>.Enumerator EntrySet()
        {
            return map.GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DirtyFlagMap<TKey, TValue>))
            {
                return false;
            }

            IDictionary targetAux = new Hashtable((IDictionary)obj);

            if (Count == targetAux.Count)
            {
                IEnumerator sourceEnum = Keys.GetEnumerator();
                while (sourceEnum.MoveNext())
                {
                    if (targetAux.Contains(sourceEnum.Current))
                    {
                        targetAux.Remove(sourceEnum.Current);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
            if (targetAux.Count == 0)
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return map.GetHashCode() ^ dirty.GetHashCode();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual ICollection<TKey> KeySet()
        {
            return new Collection.HashSet<TKey>(map.Keys);
        }

        public virtual object Put(TKey key, TValue val)
        {
            dirty = true;
            TValue tempObject;
            map.TryGetValue(key, out tempObject);
            map[key] = val;
            return tempObject;
        }

        public virtual void PutAll(IDictionary<TKey, TValue> t)
        {
            if (t != null && t.Count > 0)
            {
                dirty = true;

                List<TKey> keys = new List<TKey>(t.Keys);
                List<TValue> values = new List<TValue>(t.Values);

                for (int i = 0; i < keys.Count; i++)
                {
                    this[keys[i]] = values[i];
                }
            }
        }
    }
}