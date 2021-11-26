using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis.Sockets
{
    internal class RedisSocket : RedisSocketBase
    {
        public const int BUFFER_SIZE = 8096;

        public RedisSocket()
        {
        }

        public override async Task<byte[]> SendData(byte[] data)
        {
            await _socket.SendAsync(data, SocketFlags.None)
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

            while (IsEndOfData(data))
            {
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                data = buffer.Skip(0).Take(bytesRead).ToArray();

                result.AddRange(data);
            }

            return result.ToArray();

        }

        private bool IsEndOfData(byte[] data)
        {
            if (data.Length == 0)
                return true;

            if (data.Length == 1 && data[0] == (byte)'\n')
                return true;

            if (data[data.Length - 1] == (byte)'\n' && data[data.Length - 2] == (byte)'\n')
                return true;

            return false;
        }

    }
}
