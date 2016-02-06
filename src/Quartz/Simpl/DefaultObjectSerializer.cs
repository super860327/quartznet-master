using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using Quartz.Spi;

namespace Quartz.Simpl
{
    public class DefaultObjectSerializer : IObjectSerializer
    {
        public byte[] Serialize<T>(T obj) where T : class
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public T DeSerialize<T>(byte[] data) where T : class
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                BinaryFormatter bf = new BinaryFormatter();
                return (T)bf.Deserialize(ms);
            }
        }
    }
}