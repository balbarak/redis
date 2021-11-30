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

        protected byte[] GetRandomByteArray(int sizeInKb)
        {
            Random rnd = new Random();
            byte[] b = new byte[sizeInKb * 1024]; // convert kb to byte
            rnd.NextBytes(b);
            return b;
        }
    }
}
