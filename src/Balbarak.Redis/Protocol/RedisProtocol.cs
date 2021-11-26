using System;
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

    }
}
