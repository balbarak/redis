using MsgPack.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis.Test.Models
{
    public class MsgPackSerializer : IRedisSerializer
    {
        public T Deserialize<T>(byte[] data)
        {
            var serializer = MessagePackSerializer.Get<T>();

            T result = default;

            using (MemoryStream ms = new MemoryStream(data))
            {
                ms.Position = 0;

                result = serializer.Unpack(ms);
            }

            return result;
        }

        public byte[] Serialize(object data)
        {
            var serializer = MessagePackSerializer.Get(data.GetType());

            byte[] result = default;

            using (MemoryStream ms = new MemoryStream())
            {
                serializer.Pack(ms,data);

                ms.Position = 0;

                result = ms.ToArray();
            }

            return result;
        }
    }
}
