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
        internal async Task Should_Base64()
        {
            var key = "img";

            var client = await CreateAndConnectClient();

            var fileData = File.ReadAllBytes(@"C:\Users\balbarak\Desktop\Redis\fix.jpg");
            var fileBase64 = Convert.ToBase64String(fileData);
            var base64Leng = fileBase64.Length;

            await client.Set(key, fileBase64);

            var resultData = await client.Get(key);

            var result = Encoding.UTF8.GetString(resultData);

            var resultBase64 = Convert.FromBase64String(result);

            File.WriteAllBytes(@"C:\Users\balbarak\Desktop\Redis\output.jpg", resultBase64);
            
        }
        [Fact]
        internal async Task Should_Read_With_PipeReader()
        {
            var key = "img";

            var client = await CreateAndConnectClient();

            //var cmd = new RedisCommandBuilder("GET").WithKey(key).Build();

            var result = await client.Get(key);

            var text = Encoding.UTF8.GetString(result);


            File.WriteAllBytes(@"C:\Users\balbarak\Desktop\Redis\ff.jpg", result);
        }
    }
}
