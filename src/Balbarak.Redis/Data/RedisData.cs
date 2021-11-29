using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis.Data
{
    internal class RedisData
    {
        public byte[] RawData { get; set; }

        public string DataText => RawData != null ? Encoding.UTF8.GetString(RawData) : null;
    }
}
