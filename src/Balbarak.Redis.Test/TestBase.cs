using Balbarak.Redis.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Balbarak.Redis.Test
{
    public class TestBase
    {
        internal virtual async Task<RedisProtocol> CreateProtocolAndConnect()
        {
            var client = new RedisProtocol();

            await client.Connect(Connections.HOST, Connections.PORT);

            return client;
        }
    }
}
