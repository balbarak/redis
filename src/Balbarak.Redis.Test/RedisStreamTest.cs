using Balbarak.Redis.Data;
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
                .WithArguments(fileData)
                .Build();

            await stream.WriteAsync(setCmd);
            var result = await stream.ReadDataBlock();

            Assert.Equal("OK", result?.Result);

            stream = new RedisStream(protocol._socket);

            var getCmd = new RedisCommandBuilder("GET")
                .WithKey(key)
                .Build();

            await stream.WriteAsync(getCmd);
            result = await stream.ReadDataBlock();

            Assert.Equal(fileData, result?.ResultData);

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
                .WithArguments(fileData)
                .Build();

            await stream.WriteAsync(setCmd);
            var result = await stream.ReadDataBlock();

            Assert.Equal("OK", result?.Result);

            stream = new RedisStream(protocol._socket);

            var getCmd = new RedisCommandBuilder("GET")
                .WithKey(key)
                .Build();

            await stream.WriteAsync(getCmd);
            result = await stream.ReadDataBlock();

            Assert.Equal(fileData, result?.ResultData);

        }

        [Fact]
        public async Task Should_Set_And_Get_Large_Bytes()
        {
            var key = "largeBytes";

            var protocol = await CreateProtocolAndConnect();
            var stream = new RedisStream(protocol._socket);

            var fileData = GetRandomByteArray(30000);

            var setCmd = new RedisCommandBuilder("SET")
                .WithKey(key)
                .WithArguments(fileData)
                .Build();

            await stream.WriteAsync(setCmd);

            var result = await stream.ReadDataBlock();

            Assert.Equal("OK", result?.Result);

            stream = new RedisStream(protocol._socket);

            var getCmd = new RedisCommandBuilder("GET")
                .WithKey(key)
                .Build();


            await stream.WriteAsync(getCmd);
            var readResult = await stream.ReadDataBlock();

            Assert.Equal(fileData, readResult?.ResultData);

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
                .WithArguments(value)
                .Build();

            await stream.WriteAsync(setCmd);
            var setResult = await stream.ReadDataBlock();

            Assert.Equal("OK", setResult?.Result);
            
            stream = new RedisStream(protocol._socket);

            var getCmd = new RedisCommandBuilder("GET")
               .WithKey(key)
               .Build();

            await stream.WriteAsync(getCmd);
            var result = await stream.ReadDataBlock();

            Assert.Equal(value, result?.Result);
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
                .WithArguments(value)
                .Build();

            await stream.WriteAsync(setCmd);
            var setResult = await stream.ReadDataBlock();

            Assert.Equal("OK", setResult?.Result);
            
            stream = new RedisStream(protocol._socket);

            var getCmd = new RedisCommandBuilder("GET")
               .WithKey(key)
               .Build();

            await stream.WriteAsync(getCmd);
            var result = await stream.ReadDataBlock();

            Assert.Equal(value, result?.Result);
        }

        [Fact]
        public async Task Should_Read_Error()
        {
            var key = "KeyTest";
            var value = "W00t";

            var protocol = await CreateProtocolAndConnect();

            var stream = new RedisStream(protocol._socket);

            var setCmd = new RedisCommandBuilder("SETT")
                .WithKey(key)
                .WithArguments(value)
                .Build();

            await stream.WriteAsync(setCmd);
            var result = await stream.ReadDataBlock();

            Assert.Equal(RedisDataType.Errors, result?.Type);
        }

        [Fact]
        public async Task Should_Read_Intgers()
        {
            var key = "INTEGERKEY";

            var protocol = await CreateProtocolAndConnect();

            var stream = new RedisStream(protocol._socket);

            var setCmd = new RedisCommandBuilder("INCR")
                .WithKey(key)
                .Build();

            await stream.WriteAsync(setCmd);
            var result = await stream.ReadDataBlock();

            Assert.Equal(RedisDataType.Integers, result?.Type);
        }

        [Fact]
        public async Task Should_Read_Arrays()
        {
            var key = "list";

            var protocol = await CreateProtocolAndConnect();

            var stream = new RedisStream(protocol._socket);

            var setCmd = new RedisCommandBuilder("LRANGE")
                .WithKey(key)
                .WithArguments("0")
                .WithArguments("10")
                .Build();

            await stream.WriteAsync(setCmd);
            var result = await stream.ReadDataBlock();

            Assert.Equal(RedisDataType.Integers, result?.Type);
        }

    }
}
