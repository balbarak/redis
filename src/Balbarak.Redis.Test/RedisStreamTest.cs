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
    public class RedisStreamTest : TestBase
    {

        [Fact]
        public async Task Should_Set_Value_Bytes()
        {
            var key = "data";

            var protocol = await CreateProtocolAndConnect();
            var stream = new RedisStream(protocol._socket);
         
            //var fileData = File.ReadAllBytes(@"Data\large.jpg");
            var fileData = GetRandomByteArray(20);

            var setCmd = new RedisCommandBuilder("SET")
                .WithKey(key)
                .WithValue(fileData)
                .Build();

            await stream.WriteAsync(setCmd);
            var result = await stream.ReadRedisData();

            var getCmd = new RedisCommandBuilder("GET")
                .WithKey(key)
                .Build();

            await stream.WriteAsync(getCmd);
            result = await stream.ReadRedisData();

        }

        [Fact]
        public async Task Should_Set_And_Get_Data()
        {
            var key = "specialChar";
            var value = "السلام عليكم ورحمة الله وبركاتة \n\r welcome to !";

            var protocol = await CreateProtocolAndConnect();

            var stream = new RedisStream(protocol._socket);

            var setCmd = new RedisCommandBuilder("SET")
                .WithKey(key)
                .WithValue(value)
                .Build();

            await stream.WriteAsync(setCmd);
            var setResult = await stream.ReadRedisData();

            Assert.Equal("OK", setResult?.DataText);

            var getCmd = new RedisCommandBuilder("GET")
               .WithKey(key)
               .Build();

            await stream.WriteAsync(getCmd);
            var result = await stream.ReadRedisData();

            Assert.Equal(value, result?.DataText);
        }

        [Fact]
        public async Task Should_Get_Redis_Data()
        {
            var key = "img";

            var protocol = await CreateProtocolAndConnect();

            var stream = new RedisStream(protocol._socket);

            var cmd = new RedisCommandBuilder("GET")
                .WithKey(key)
                .Build();

            await stream.WriteAsync(cmd);

            var buffer = await stream.ReadRedisBuffer();
        }

        private byte[] GetRandomByteArray(int sizeInKb)
        {
            Random rnd = new Random();
            byte[] b = new byte[sizeInKb * 1024]; // convert kb to byte
            rnd.NextBytes(b);
            return b;
        }
    }
}
