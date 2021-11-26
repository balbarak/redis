using Balbarak.Redis.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Balbarak.Redis.Test
{
    public class RedisProtocolTest
    {
        [Fact]
        public async Task Should_Set_Data()
        {
            var client = new RedisProtocol();

            await client.Connect(Connections.HOST, Connections.PORT);

            var dataToSend = Encoding.UTF8.GetBytes("set hel \"This is stored using redis client\" \n");

            var dataRcieved = await client.SendCommand(dataToSend);

            var result = Encoding.UTF8.GetString(dataRcieved);

            Assert.Equal(RedisResponse.SUCCESS,result);
        }

        [Fact]
        public async Task Should_Read_Data()
        {
            var client = new RedisProtocol();

            await client.Connect(Connections.HOST, Connections.PORT);

            var dataToSend = Encoding.UTF8.GetBytes("get test \n");

            var dataRecieved = await client.SendCommand(dataToSend);

            var result = Encoding.UTF8.GetString(dataRecieved);

        }

        [Fact]
        public async Task Should_Determine_Error()
        {
            var client = new RedisProtocol();

            await client.Connect(Connections.HOST, Connections.PORT);

            var dataToSend = Encoding.UTF8.GetBytes("smembers test \n");

            var dataRecieved = await client.SendCommand(dataToSend);

            var text = Encoding.UTF8.GetString(dataRecieved);

            Assert.True(client.HasError(dataRecieved));

        }

        [Fact]
        public async Task Should_Set_Large_Data()
        {
            var client = new RedisProtocol();
            await client.Connect(Connections.HOST, Connections.PORT);

            var dataToSend = File.ReadAllBytes(@"C:\Users\balbarak\Desktop\Tools\cpuz_x32.exe");

            var base64str = Convert.ToBase64String(dataToSend);
            
            var cmd = Encoding.UTF8.GetBytes($"SET fork ");

            var data = new List<byte>();

            data.AddRange(cmd);
            data.AddRange(dataToSend);

            data.AddRange(Encoding.UTF8.GetBytes(" \n\n\n\n"));

            await client.SendCommand(data.ToArray());


        }
    }
}
