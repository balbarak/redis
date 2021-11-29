using Balbarak.Redis.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis.Test
{
    internal class RedisStreamTest : TestBase
    {
        public async Task Should_Test_Stream()
        {
            var protocol = await CreateProtocolAndConnect();

            var stream = new RedisStream(protocol._socket);
        }
    }
}
