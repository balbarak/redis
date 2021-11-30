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
        public async Task Should_Set_And_Get_File_Bytes()
        {
            var key = "data";

            var protocol = await CreateProtocolAndConnect();
            var stream = new RedisStream(protocol._socket);

            var fileData = File.ReadAllBytes(@"Data\large.jpg");

            var setCmd = new RedisCommandBuilder("SET")
                .WithKey(key)
                .WithValue(fileData)
                .Build();

            await stream.WriteAsync(setCmd);
            var result = await stream.ReadRedisData();

            Assert.Equal("OK", result?.DataText);

            var getCmd = new RedisCommandBuilder("GET")
                .WithKey(key)
                .Build();

            await stream.WriteAsync(getCmd);
            result = await stream.ReadRedisData();

            Assert.Equal(fileData, result?.Data);

        }

        [Fact]
        public async Task Should_Set_And_Get_Bytes()
        {
            var key = "randomBytes";

            var protocol = await CreateProtocolAndConnect();
            var stream = new RedisStream(protocol._socket);

            var fileData = GetRandomByteArray(4096);

            var setCmd = new RedisCommandBuilder("SET")
                .WithKey(key)
                .WithValue(fileData)
                .Build();

            await stream.WriteAsync(setCmd);
            var result = await stream.ReadRedisData();

            Assert.Equal("OK", result?.DataText);

            var getCmd = new RedisCommandBuilder("GET")
                .WithKey(key)
                .Build();

            await stream.WriteAsync(getCmd);
            result = await stream.ReadRedisData();

            Assert.Equal(fileData, result?.Data);

        }

        [Fact]
        public async Task Should_Set_And_Get_Strings()
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
        public async Task Should_Set_And_Get_Strings_When_Key_Not_ASCII()
        {
            var key = "السلام عليكم";
            var value = "وعليكم السلام ورحمة الله وبركاتة";

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


        private byte[] GetRandomByteArray(int sizeInKb)
        {
            Random rnd = new Random();
            byte[] b = new byte[sizeInKb * 1024]; // convert kb to byte
            rnd.NextBytes(b);
            return b;
        }
    }
}
