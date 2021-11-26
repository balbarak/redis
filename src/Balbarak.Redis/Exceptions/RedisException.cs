using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis
{

    [Serializable]
    public class RedisException : Exception
    {
        public RedisException() { }
        public RedisException(string message) : base(message) { }
        public RedisException(string message, Exception inner) : base(message, inner) { }
        protected RedisException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

    }
}
