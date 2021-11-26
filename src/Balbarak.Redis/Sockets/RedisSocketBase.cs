using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis.Sockets
{
    internal abstract class RedisSocketBase : IAsyncDisposable
    {
        protected Socket _socket;

        public RedisSocketBase()
        {
            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        public virtual async Task Connect(string host, int port)
        {
            await _socket.ConnectAsync(host, port);
        }

        public abstract Task<byte[]> SendData(byte[] data);

        public ValueTask DisposeAsync()
        {
            if (_socket != null)
                _socket.Dispose();

            return ValueTask.CompletedTask;
        }
    }
}
