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
            var client = await CreateAndConnectClient();

            var data = "This is a test message from redis protocol client !";

            var result = await client.Set("text", data);

            Assert.True(result);
        }

        [Fact]
        public async Task Should_Set_And_Get_Data()
        {
            var client = await CreateAndConnectClient();

            var key = "text";

            var data = "This is a test message from redis protocol client !";

            var result = await client.Set(key, data);

            Assert.True(result);

            var dataRecieved = await client.Get("SS");

            var dataRecievedText = Encoding.UTF8.GetString(dataRecieved);

        }

        [Fact]
        public async Task Should_Set_And_Get_BulkStrings()
        {
            var client = await CreateAndConnectClient();

            var key = "text";

            var data = "This is a test message from redis protocol client 333!";

            var result = await client.Set(key, data);

            Assert.True(result);

            var dataRecieved = await client.GetBulkStrings(key);

            Assert.Equal(data, dataRecieved);

        }

        [Fact]
        public async Task Should_Return_Null_When_No_Data()
        {
            var client = await CreateAndConnectClient();

            var result = await client.GetBulkStrings(Guid.NewGuid().ToString().ToLower());

            Assert.Empty(result);
        }

        private async Task<RedisProtocol> CreateAndConnectClient()
        {
            var client = new RedisProtocol();

            await client.Connect(Connections.HOST, Connections.PORT);

            return client;
        }
    }
}
