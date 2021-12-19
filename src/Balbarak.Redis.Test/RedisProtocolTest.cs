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
    public class RedisProtocolTest : TestBase
    {
        [Fact]
        public async Task Should_Auth_With_User_And_Password()
        {
            var client = await CreateAndConnectClientAuth();

            var authResult = await client.Auth("admin", "1122");

            ConfirmSuccessResult(authResult);

        }

        [Fact]
        public async Task Should_Not_Auth_When_User_And_Password_Is_Not_Valid()
        {
            var client = await CreateAndConnectClientAuth();

            var authResult = await client.Auth("admins", "1122");

            Assert.Equal(RedisDataType.Errors, authResult.Type);

            Assert.Equal(RedisResponse.AUTH_FAILED, authResult?.Result);

        }

        [Fact]
        public async Task Should_Set_Data_With_Special_Characters()
        {
            var client = await CreateAndConnectClient();

            var key = "text";

            var data = "This data has \n\r with protocl characters ! and end with \n\r";

            var setResult = await client.Set(key, data);

            ConfirmSuccessResult(setResult);

            var dataRecieved = await client.Get(key);

            Assert.Equal(data, dataRecieved?.Result);
        }

        [Fact]
        public async Task Should_Set_Data()
        {
            var client = await CreateAndConnectClient();

            var data = "This is a test message from redis protocol client !";

            var result = await client.Set("text", data);

            ConfirmSuccessResult(result);
        }

        [Fact]
        public async Task Should_Set_And_Get_BulkStrings()
        {
            var client = await CreateAndConnectClient();

            var key = "text";

            var data = "This is a test message from redis protocol client 333!";

            var result = await client.Set(key, data);

            ConfirmSuccessResult(result);

            var dataRecieved = await client.Get(key);

            Assert.Equal(data, dataRecieved?.Result);

        }

        [Fact]
        public async Task Should_Return_Null_When_No_Data()
        {
            var client = await CreateAndConnectClient();

            var result = await client.Get(Guid.NewGuid().ToString().ToLower());

            Assert.Empty(result?.Result);
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

            var result = await client.Set(key, "hello world");

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

            var result = await client.Set(key, data);

            ConfirmSuccessResult(result);

            var dataRecieved = await client.Get(key);

            Assert.Equal(data, dataRecieved?.Result);
        }

        [Fact]
        public async Task Should_Set_And_Get_Bytes()
        {
            var key = "dataBytesFile";

            var client = await CreateAndConnectClient();

            var data = await File.ReadAllBytesAsync(@"Data\large.jpg");

            var result = await client.Set(key, data);

            ConfirmSuccessResult(result);

            var dataRecieved = await client.Get(key);

            Assert.Equal(data, dataRecieved?.ResultData);
        }

        [Fact]
        public async Task Should_Set_And_Get_Large_Bytes()
        {
            //var key = "large data bytes";

            //var client = await CreateAndConnectClient();

            //var data = GetRandomByteArray(90000);

            //var result = await client.Set(key, data);

            //ConfirmSuccessResult(result);

            //var dataRecieved = await client.Get(key);

            //Assert.Equal(data, dataRecieved?.ResultData);
        }

        [Fact]
        public async Task Should_Set_Expiration()
        {
            var key = "keyWithExpired";
            var value = "This will expire in 2 seconds ...";

            var expire = TimeSpan.FromSeconds(2);

            var client = await CreateAndConnectClient();

            var result = await client.Set(key, value, expire);

            ConfirmSuccessResult(result);

            await Task.Delay(2000);

            var dataRecieved = await client.Get(key);

            Assert.Empty(dataRecieved?.Result);
        }

        [Fact]
        public async Task Should_Ping()
        {
            var client = await CreateAndConnectClient();

            var result = await client.Ping();

            Assert.Equal("PONG", result.Result);
        }

        [Fact]
        public async Task Should_Read_Info()
        {
            var client = await CreateAndConnectClient();

            var result = await client.Info();

            //Assert.Equal("PONG", result.Result);
        }

        [Fact]
        public async Task Should_Delete_Key()
        {

            var client = await CreateAndConnectClient();

            await client.Set("k1", "this gonna be deleted !");
            await client.Set("k2", "this gonna be deleted !");
            await client.Set("k3", "this gonna be deleted !");
            await client.Set("k4", "this gonna be deleted !");
            await client.Set("k5", "this gonna be deleted !");

            var result = await client.Del("k1","k2","k3","k4","k5");

            Assert.Equal(RedisDataType.Integers, result.Type);
            
        }

        private async Task<RedisProtocol> CreateAndConnectClient()
        {
            var client = new RedisProtocol();

            await client.Connect(Connections.HOST, Connections.PORT);

            return client;
        }

        private async Task<RedisProtocol> CreateAndConnectClientAuth()
        {
            var client = new RedisProtocol();

            await client.Connect(Connections.HOST, Connections.AUTH_PORT);

            return client;
        }

        private void ConfirmSuccessResult(RedisDataBlock result)
        {
            Assert.Equal(RedisDataType.SimpleStrings, result?.Type);

            Assert.Equal(RedisResponse.OK, result.Result);
        }

        private void ConfirmTrueResult(RedisDataBlock result)
        {
            Assert.Equal(RedisDataType.Integers, result?.Type);

            Assert.Equal("1", result.Result);
        }

        private void ConfirmFalseResult(RedisDataBlock result)
        {
            Assert.Equal(RedisDataType.Integers, result?.Type);

            Assert.Equal("0", result.Result);
        }
    }
}
