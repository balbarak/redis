using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis
{
    internal enum RedisDataType
    {
        Integers,
        SimpleString,
        BulkString,
        Error,
        Arrays,
        Unkown
    }
}
