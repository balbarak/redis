using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Balbarak.Redis
{
    internal class RedisJsonSerializer : IRedisSerializer
    {
        public byte[] Serialize(object data)
        {
            var result = JsonSerializer.Serialize(data);

            return Encoding.UTF8.GetBytes(result);
        }

        public T Deserialize<T>(byte[] data)
        {
            return JsonSerializer.Deserialize<T>(data);
        }
    }
}
