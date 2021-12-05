using Balbarak.Redis.Test.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Balbarak.Redis.Test.Serialization
{
    public class RedisJsonSerializerTest : TestBase
    {
        [Fact]
        public void Should_Serialize_And_Deserialze_Object()
        {
            var serializer = new RedisJsonSerializer();

            var employe = Employee.Create();

            var dataSerialized = serializer.Serialize(employe);

            var result = serializer.Deserialize<Employee>(dataSerialized);

            Assert.True(employe.Equals(result));
        }
    }
}
