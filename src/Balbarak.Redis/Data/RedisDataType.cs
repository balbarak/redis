using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis.Data
{
    internal enum RedisDataType
    {
        SimpleStrings,
        Integers,
        Arrays,
        Errors,
        BulkStrings
}
}
