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

        public string RawDataText => RawData != null ? Encoding.UTF8.GetString(RawData) : null;

        public byte[] Data { get; set; }

        public string DataText => Data != null ? Encoding.UTF8.GetString(Data) : null;

        public RedisData()
        {

        }
    }
}
