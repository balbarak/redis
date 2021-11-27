using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis.Protocol
{
    internal enum RedisProtocolDataType
    {
        Integers,
        BulkStrings,
        Arrays,
        SimpleStrings,
        Errors,
        Unkown
    }
}
