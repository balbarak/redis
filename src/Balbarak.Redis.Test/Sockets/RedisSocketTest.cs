using Balbarak.Redis.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Balbarak.Redis.Test
{
    public class RedisSocketTest
    {
        [Fact]
        public async Task Should_Set_Data()
        {
            var client = new RedisSocket();

            await client.Connect(Connections.HOST, Connections.PORT);

            var dataToSend = Encoding.UTF8.GetBytes("set hel \"This is stored using redis client\" \n");

            var dataRcieved = await client.SendData(dataToSend);

            var result = Encoding.UTF8.GetString(dataRcieved);

            Assert.Equal(RedisResponse.SUCCESS,result);
        }

        [Fact]
        public async Task Should_Read_Data()
        {
            var client = new RedisSocket();

            await client.Connect(Connections.HOST, Connections.PORT);

            var dataToSend = Encoding.UTF8.GetBytes("get test \n");

            var dataRecieved = await client.SendData(dataToSend);

            var result = Encoding.UTF8.GetString(dataRecieved);

        }

        [Fact]
        public async Task Should_Determine_Error()
        {
            var client = new RedisSocket();

            await client.Connect(Connections.HOST, Connections.PORT);

            var dataToSend = Encoding.UTF8.GetBytes("smembers test \n");

            var dataRecieved = await client.SendData(dataToSend);

            var text = Encoding.UTF8.GetString(dataRecieved);

            Assert.True(client.HasError(dataRecieved));

        }
    }
}
