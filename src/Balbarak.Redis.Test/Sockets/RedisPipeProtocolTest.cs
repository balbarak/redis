using Balbarak.Redis.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Balbarak.Redis.Test
{
    public class RedisPipeProtocolTest
    {
        [Fact]
        public async Task Should_Send_Data()
        {
            var host = "localhost";
            var port = 6379;

            var redisSocket = new RedisPipeProtocol();

            await redisSocket.Connect(host, port);

            await redisSocket.SendCommand(Encoding.UTF8.GetBytes("set list fe \n"));
            
        }
    }
}
