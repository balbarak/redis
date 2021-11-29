using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis.Data
{
    internal class BulkString
    {
        public byte[] Data { get; set; }

        public long Size { get; set; }

        public string Value => Encoding.UTF8.GetString(Data);

        public BulkString(byte[] data)
        {

        }
    }
}
