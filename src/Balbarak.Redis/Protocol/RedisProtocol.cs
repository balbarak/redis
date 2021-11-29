using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace Balbarak.Redis.Protocol
{
    internal class RedisProtocol
    {
        protected Socket _socket;

        public RedisProtocol()
        {
            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        public async Task Connect(string host, int port)
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

        public async Task<bool> Set(string key, string value)
        {
            var dataToSend = new RedisCommandBuilder("SET")
                .WithKey(key)
                .WithValue(value)
                .Build();

            var redisRawData = await SendCommandInternal(dataToSend);

            ValidateError(redisRawData);

            var resultText = Encoding.UTF8.GetString(redisRawData);

            return resultText == RedisResponse.OK;
        }

        public async Task<byte[]> Get(string key)
        {
            var dataToSend = new RedisCommandBuilder("GET")
                .WithKey(key)
                .Build();

            var rawData = await SendCommandInternal(dataToSend)
                .ConfigureAwait(false);

            return rawData;
        }

        public async Task<bool> Exists(string key)
        {
            var dataToSend = new RedisCommandBuilder("EXISTS")
              .WithKey(key)
              .Build();

            var redisRawData = await SendCommandInternal(dataToSend);

            var result = ReadInteger(ref redisRawData);

            if (result == 0)
                return false;
            else
                return true;
        }

        public async Task<string> Ping()
        {
            var dataToSend = $"PING \n".ToUTF8Bytes();

            var result = await SendCommandInternal(dataToSend);

            return Encoding.UTF8.GetString(result);
        }

        private ReadOnlySpan<byte> ReadData(ref byte[] redisRawData)
        {
            var span = new ReadOnlySpan<byte>(redisRawData);

            var size = ReadDataSize(ref redisRawData, out int startIndex);

            return span.Slice(startIndex, size);
        }

        private int ReadDataSize(ref byte[] redisRawData, out int startIndex)
        {
            var span = new ReadOnlySpan<byte>(redisRawData);
            var size = new ReadOnlySpan<byte>();

            startIndex = 0;

            for (int i = 0; i < redisRawData.Length; i++)
            {
                var current = redisRawData[i];

                if (i == 0 && (current == (byte)'$' || current == (byte)'-'))
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

        private int ReadInteger(ref byte[] redisRawData)
        {
            if (redisRawData == null)
                throw new RedisException("There is no data to parse");

            if (redisRawData[0] != (byte)':')
                throw new RedisException("Data recieved is not integer");

            var text = Encoding.UTF8.GetString(redisRawData, 1, redisRawData.Length - 3);

            int.TryParse(text, out int result);

            return result;
        }

        private async Task<byte[]> SendCommandInternal(byte[] data)
        {
            var stream = new RedisStream(_socket);

            var sentBytes = await _socket.SendAsync(data, SocketFlags.None)
                .ConfigureAwait(false);

            return await stream.ReadRedisBuffer();
        }

        private void ValidateError(byte[] redisRawData)
        {
            if (redisRawData == null)
                throw new RedisException("No data recieved from redis");

            if (redisRawData[0] == (byte)'-')
            {
                var errorMessage = Encoding.UTF8.GetString(redisRawData, 1, redisRawData.Length - 3);

                throw new RedisException(errorMessage);
            }
        }
    }
}
