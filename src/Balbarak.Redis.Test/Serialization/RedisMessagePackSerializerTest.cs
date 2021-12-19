using Balbarak.Redis.Test.Models;
using MsgPack.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Balbarak.Redis.Test.Serialization
{
    public class RedisMessagePackSerializerTest
    {
        [Fact]
        public void Should_Serialize_And_Deserialize()
        {
            var serializer = MessagePackSerializer.Get<Employee>();

            var employee = Employee.Create();

            var jsonEmployee = JsonConvert.SerializeObject(employee);

            var dataNotPacked = Encoding.UTF8.GetBytes(jsonEmployee);

            using (MemoryStream ms = new MemoryStream())
            {
                serializer.Pack(ms, employee);

                var dataPacked = ms.ToArray();

                ms.Position = 0;
                var employeeUnpacked = serializer.Unpack(ms);
            }
        }
    }
}
