using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis.Protocol
{
    internal class RedisProtocol : RedisProtocolBase
    {
        public const int BUFFER_SIZE = 8096;

        public RedisProtocol()
        {
        }

        public async Task<bool> Set(string key, string value)
        {
            var cmd = $"SET {key} \"{value}\" \n";

            var data = cmd.ToUTF8Bytes();

            var result = await SendCommand(data);

            var resultText = Encoding.UTF8.GetString(result);

            return resultText == RedisResponse.SUCCESS;
        }

        public async Task<bool> Set(string key,byte[] value)
        {
            var valueBase64 = Convert.ToBase64String(value);

            var cmd = $"SET {key} \"{valueBase64}\" \n";

            var data = cmd.ToUTF8Bytes();

            var result = await SendCommand(data);

            var resultText = Encoding.UTF8.GetString(result);

            return resultText == RedisResponse.SUCCESS;
        }

        public async Task<byte[]> Get(string key)
        {
            var cmd = $"GET {key} \n";

            var data = cmd.ToUTF8Bytes();

            var result = await SendCommand(data);

            return result;
        }

        public async Task<string> GetBulkStrings(string key)
        {
            var dataToSend = $"GET {key} \n".ToUTF8Bytes();

            var result = await SendCommand(dataToSend)
                .ConfigureAwait(false);

            if (result.Length == 0)
                throw new RedisException();


            return ReadBulkStrings(result);
        }

        public override async Task<byte[]> SendCommand(byte[] data)
        {
            var sentBytes = await _socket.SendAsync(data, SocketFlags.None)
                .ConfigureAwait(false);

            return await ReadDataInternal()
                .ConfigureAwait(false);
        }

        private async Task<byte[]> ReadDataInternal()
        {
            var result = new List<byte>();

            var stream = new NetworkStream(_socket);

            var buffer = new byte[BUFFER_SIZE];

            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

            var data = buffer.Skip(0).Take(bytesRead).ToArray();

            result.AddRange(data);

            while (!IsEndOfData(data))
            {
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                data = buffer.Skip(0).Take(bytesRead).ToArray();

                result.AddRange(data);
            }

            return result.ToArray();

        }

        private string ReadBulkStrings(byte[] redisRawData)
        {
            var data = ReadData(redisRawData);

            return Encoding.UTF8.GetString(data);
        }

        private ReadOnlySpan<byte> ReadData(byte[] redisRawData)
        {
            var span = new ReadOnlySpan<byte>(redisRawData);

            var size = ReadDataSize(redisRawData, out int startIndex);

            return span.Slice(startIndex, size);
        }

        private int ReadDataSize(byte[] redisRawData,out int startIndex)
        {
            var span = new ReadOnlySpan<byte>(redisRawData);
            var size = new ReadOnlySpan<byte>();

            startIndex = 0;

            for (int i = 0; i < redisRawData.Length; i++)
            {
                var current = redisRawData[i];

                if (i == 0 && current == (byte)'$')
                    continue;

                if (i == 1 && current == (byte)'-')
                    return 0;

                if (current == (byte)'\r')
                {
                    startIndex = i + 2;

                    size = span.Slice(1, i - 1);

                    break;
                }
            }

            var sizeText = Encoding.UTF8.GetString(size);

            Int32.TryParse(sizeText, out int result);

            return result;


        }

    }
}
