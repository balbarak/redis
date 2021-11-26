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

        public bool HasError(byte[] data)
        {
            if (data == null)
                return false;

            if (data[0] == (byte)'-')
                return true;

            return false;
        }

        public RedisDataType GetType(byte[] data)
        {
            if (data == null)
                return RedisDataType.Unkown;

            if (data[0] == (byte)'-')
                return RedisDataType.Error;

            if (data[0] == (byte)'+')
                return RedisDataType.SimpleString;

            if (data[0] == (byte)':')
                return RedisDataType.Integers;

            if (data[0] == (byte)'$')
                return RedisDataType.BulkString;

            if (data[0] == (byte)'*')
                return RedisDataType.Arrays;

            return RedisDataType.Unkown;
        }

        public ValueTask DisposeAsync()
        {
            if (_socket != null)
                _socket.Dispose();

            return ValueTask.CompletedTask;
        }
    }
}
