using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis
{
    public interface IRedisSerializer
    {
        public byte[] Serialize(object data);

        public T Deserialize<T>(byte[] data);
    }
}
