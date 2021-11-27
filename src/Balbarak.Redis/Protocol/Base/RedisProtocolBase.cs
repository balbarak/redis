using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis.Protocol
{
    internal abstract class RedisProtocolBase : IAsyncDisposable
    {
        protected Socket _socket;

        public RedisProtocolBase()
        {
            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        public virtual async Task Connect(string host, int port)
        {
            try
            {
                await _socket.ConnectAsync(host, port);
            }
            catch (Exception ex)
            {
                throw new RedisException($"Unable to connect to redis server {host}:{port}", ex);
            }
        }
        
        public ValueTask DisposeAsync()
        {
            if (_socket != null)
                _socket.Dispose();

            return ValueTask.CompletedTask;
        }

        protected RedisDataType GetType(byte[] data)
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

        protected bool IsEndOfData(byte[] data)
        {
            if (data == null)
                return true;

            if (data.Length == 0)
                return true;

            if (data.Length == 1 && data[0] == (byte)'\n')
                return true;

            if (data[data.Length - 2] == (byte)'\r' && data[data.Length - 1] == (byte)'\n')
                return true;

            return false;
        }
    }
}
