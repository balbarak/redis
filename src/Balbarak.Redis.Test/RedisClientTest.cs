using Balbarak.Redis.Test.Models;
using Balbarak.Redis.Test.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Balbarak.Redis.Test
{
    public class RedisClientTest : TestBase
    {
        [Fact]
        public async Task Should_Use_Custom_Serializer()
        {
            var key = "msgPack";

            var client = await CreateClient();

            client.Settings.Serializer = new MsgPackSerializer();

            var employee = Employee.Create();

            var setResult = await client.Set(key, employee);

            Assert.True(setResult);

            var result = await client.Get<Employee>(key);


            Assert.True(result.Equals(employee));
        }

        [Fact]
        public async Task Should_Set_Strings()
        {
            var key = "from client !";
            var value = "Hello this is set from redis client !";

            var client = await CreateClient();

            var result = await client.Set(key, value);

            Assert.True(result);
        }

        [Fact]
        public async Task Should_Set_And_Get_Strings()
        {
            var key = "\n\rKey\r\nImpos\r\n";
            var value = "Hello this is set from redis client !";

            var client = await CreateClient();

            var setResult = await client.Set(key, value);

            Assert.True(setResult);

            var result = await client.GetString(key);

            Assert.Equal(value, result);
        }

        [Fact]
        public async Task Should_Set_And_Get_Bytes()
        {
            var key = "ClientBytes";
            var value = GetRandomByteArray(10);

            var client = await CreateClient();

            var setResult = await client.Set(key, value);

            Assert.True(setResult);

            var result = await client.GetBytes(key);

            Assert.Equal(value, result);
        }

        [Fact]
        public async Task Should_Set_And_Get_SerializeData()
        {
            var key = "serialized";

            var client = await CreateClient();

            var employee = Employee.Create();

            var setResult = await client.Set(key, employee);

            Assert.True(setResult);

            var result = await client.Get<Employee>(key);


            Assert.True(result.Equals(employee));
        }

        [Fact]
        public async Task Should_Delete_Key()
        {
            var key = "keyToBeDeleted";

            var client = await CreateClient();

            var setResult = await client.Set(key, "Data to be deleted !");

            Assert.True(setResult);

            var result = await client.Delete(key);

            Assert.Equal(1, result);

        }

        [Fact]
        public async Task Should_Delete_Keys()
        {
            var client = await CreateClient();

            await client.Set("k1", "Data to be deleted !");
            await client.Set("k2", "Data to be deleted !");
            await client.Set("k3", "Data to be deleted !");
            await client.Set("k4", "Data to be deleted !");

            var result = await client.Delete("k1", "k2", "k3", "k4");

            Assert.Equal(4, result);

        }

        [Fact]
        public async Task Should_Serialze_And_Desrialze_Using_NewtonSoft()
        {
            var key = "newtonsoft";

            var client = await CreateClient();

            client.Settings.Serializer = new RedisNewtonSoftSerializer();

            var employee = Employee.Create();

            var setResult = await client.Set(key,employee);

            Assert.True(setResult);

            var result = await client.Get<Employee>(key);

            Assert.True(result.Equals(employee));
        }

    }
}
