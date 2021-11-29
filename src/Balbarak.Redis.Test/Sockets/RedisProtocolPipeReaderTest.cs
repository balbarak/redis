using Balbarak.Redis.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Balbarak.Redis.Test.Sockets
{
    public class RedisProtocolPipeReaderTest : TestBase
    {
        [Fact]
        internal async Task Should_Read_With_PipeReader()
        {
            var key = "img";

            var client = await CreateAndConnectClient();

            //var cmd = new RedisCommandBuilder("GET").WithKey(key).Build();

            var result = await client.Get(key);

        }
    }
}
