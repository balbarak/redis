using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis.Test.Serialization
{
    internal class RedisNewtonSoftSerializer : IRedisSerializer
    {
        public T Deserialize<T>(byte[] data)
        {
            var json = Encoding.UTF8.GetString(data);

            return JsonConvert.DeserializeObject<T>(json);
        }

        public byte[] Serialize(object data)
        {
            var json = JsonConvert.SerializeObject(data);

            return Encoding.UTF8.GetBytes(json);
        }
    }
}
