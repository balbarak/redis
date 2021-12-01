﻿using Balbarak.Redis.Data;
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
    public class RedisProtocolTest : TestBase
    {
        [Fact]
        public async Task Should_Set_Data_With_Special_Characters()
        {
            var client = await CreateAndConnectClient();

            var key = "text";

            var data = "This data has \n\r with protocl characters ! and end with \n\r";

            var setResult = await client.SetString(key, data);

            ConfirmSuccessResult(setResult);

            var dataRecieved = await client.GetString(key);

            Assert.Equal(data, dataRecieved?.DataText);
        }

        [Fact]
        public async Task Should_Set_Data()
        {
            var client = await CreateAndConnectClient();

            var data = "This is a test message from redis protocol client !";

            var result = await client.SetString("text", data);

            ConfirmSuccessResult(result);
        }

        [Fact]
        public async Task Should_Set_And_Get_BulkStrings()
        {
            var client = await CreateAndConnectClient();

            var key = "text";

            var data = "This is a test message from redis protocol client 333!";

            var result = await client.SetString(key, data);

            ConfirmSuccessResult(result);

            var dataRecieved = await client.GetString(key);

            Assert.Equal(data, dataRecieved?.DataText);

        }

        [Fact]
        public async Task Should_Return_Null_When_No_Data()
        {
            var client = await CreateAndConnectClient();

            var result = await client.GetString(Guid.NewGuid().ToString().ToLower());

            Assert.Empty(result?.DataText);
        }

        [Fact]
        public async Task Should_Return_False_When_No_Key()
        {
            var client = await CreateAndConnectClient();

            var result = await client.Exists(Guid.NewGuid().ToString().ToLower());

            ConfirmFalseResult(result);
        }

        [Fact]
        public async Task Should_Return_True_When_There_Is_Key()
        {
            var client = await CreateAndConnectClient();
            
            var key = "welcomekey";

            var result = await client.SetString(key, "hello world");

            ConfirmSuccessResult(result);

            var existResult = await client.Exists(key);

            ConfirmTrueResult(existResult);
        }
        
        [Fact]
        public async Task Should_Set_Base_64_Data()
        {
            var key = "img";

            var client = await CreateAndConnectClient();

            var dataBytes = await File.ReadAllBytesAsync(@"Data\large.jpg");

            var data = Convert.ToBase64String(dataBytes);

            var result = await client.SetString(key, data);

            ConfirmSuccessResult(result);

            var dataRecieved = await client.GetString(key);

            Assert.Equal(data, dataRecieved?.DataText);
        }

        [Fact]
        public async Task Should_Set_And_Get_Bytes()
        {
            var key = "dataBytesFile";

            var client = await CreateAndConnectClient();

            var data = await File.ReadAllBytesAsync(@"Data\large.jpg");

            var result = await client.SetBytes(key, data);

            ConfirmSuccessResult(result);

            var dataRecieved = await client.GetBytes(key);

            Assert.Equal(data, dataRecieved?.Data);
        }

        [Fact]
        public async Task Should_Set_And_Get_Large_Bytes()
        {
            var key = "large data bytes";

            var client = await CreateAndConnectClient();

            var data = GetRandomByteArray(90000);

            var result = await client.SetBytes(key, data);

            ConfirmSuccessResult(result);

            var dataRecieved = await client.GetBytes(key);

            Assert.Equal(data, dataRecieved?.Data);
        }

        [Fact]
        public async Task Should_Ping()
        {
            var client = await CreateAndConnectClient();

            var result = await client.Ping();

            Assert.Equal("PONG", result.DataText);
        }

        private async Task<RedisProtocol> CreateAndConnectClient()
        {
            var client = new RedisProtocol();

            await client.Connect(Connections.HOST, Connections.PORT);

            return client;
        }

        private void ConfirmSuccessResult(RedisDataBlock result)
        {
            Assert.Equal(RedisDataType.SimpleStrings, result?.DataType);

            Assert.Equal(RedisResponse.OK, result.DataText);
        }

        private void ConfirmTrueResult(RedisDataBlock result)
        {
            Assert.Equal(RedisDataType.Integers, result?.DataType);

            Assert.Equal("1", result.DataText);
        }

        private void ConfirmFalseResult(RedisDataBlock result)
        {
            Assert.Equal(RedisDataType.Integers, result?.DataType);

            Assert.Equal("0", result.DataText);
        }
    }
}
