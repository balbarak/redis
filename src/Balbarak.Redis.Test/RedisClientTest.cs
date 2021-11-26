using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Balbarak.Redis.Test
{
    
    public class RedisClientTest
    {
        [Fact]
        public async Task Should_Connect()
        {
            var host = "localhost";
            var port = 6379;

            var redisClient = new RedisClient(host, port);

            await redisClient.Connect();

            await redisClient.Ping();
        }
    }
}
