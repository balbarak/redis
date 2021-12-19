using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis.Models
{
    public class RedisServer
    {
        public string Version { get; set; }

        public string ConfigFile { get; set; }

        public TimeSpan UpTime { get; set; }

        public long UsedMemeory { get; set; }


    }
}
