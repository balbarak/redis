using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis.Data
{
    internal class RedisDataBlock
    {
        public byte[] RawData { get;private set; }

        public string RawDataText => RawData != null ? Encoding.UTF8.GetString(RawData) : null;

        public byte[] Data { get;private set; }

        public string DataText => Data != null ? Encoding.UTF8.GetString(Data) : null;

        public RedisDataType DataType { get; private set; }

        public RedisDataBlock(RedisDataType type,byte[] rawData,byte[] data)
        {
            DataType = type;
            RawData = rawData;
            Data = data;
        }
    }
}
