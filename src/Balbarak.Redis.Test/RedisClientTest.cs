using Balbarak.Redis.Test.Models;
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

            var result = await client.GetStrings(key);

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
    }
}
