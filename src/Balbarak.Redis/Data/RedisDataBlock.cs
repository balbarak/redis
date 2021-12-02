using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis.Data
{
    internal class RedisDataBlock
    {
        public byte[] RawData { get;private set; }

        public string RawDataText { get; private set; }

        public byte[] ResultData { get;private set; }

        public string Result { get; private set; }

        public RedisDataType Type { get; private set; }

        public RedisDataBlock(RedisDataType type,byte[] rawData,byte[] data)
        {
            Type = type;
            RawData = rawData;
            ResultData = data;

            SetUTF8Text();
        }

        public RedisDataBlock(RedisDataType type, ReadOnlySequence<byte> rawData,SequencePosition dataStart,SequencePosition dateEnd)
        {
            Type = type;
            RawData = rawData.ToArray();
            ResultData = rawData.Slice(dataStart, dateEnd).ToArray();

            SetUTF8Text();
        }
        private void SetUTF8Text()
        {
            if (ResultData != null)
            {
                Result = Encoding.UTF8.GetString(ResultData);
            }

            if (RawData != null)
            {
                RawDataText = Encoding.UTF8.GetString(RawData);
            }
        }

    }
}
