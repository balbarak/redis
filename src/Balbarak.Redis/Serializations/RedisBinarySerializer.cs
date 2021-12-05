using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis
{
    internal class RedisBinarySerializer : IRedisSerializer
    {
        public T Deserialize<T>(byte[] data)
        {
            throw new NotImplementedException();
        }

        public byte[] Serialize(object data)
        {
            throw new NotImplementedException();
        }
    }
}
