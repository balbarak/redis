using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis
{
    internal class RedisProtocolDataTypes
    {
        public const byte BULK_STRINGS = (byte)'$';
        public const byte ARRAYS = (byte)'*';
        public const byte ERRORS = (byte)'-';
        public const byte SIMPLE_STRINGS = (byte)'+';
        public const byte INTEGERS = (byte)':';
    }
}
