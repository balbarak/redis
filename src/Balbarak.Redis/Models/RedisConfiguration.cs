using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis
{
    public class RedisConfiguration
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public IRedisSerializer Serializer { get; set; } = new RedisJsonSerializer();

        public RedisConfiguration()
        {

        }

        public RedisConfiguration(string host,int port)
        {
            Host = host;
            Port = port;
        }

    }
}
