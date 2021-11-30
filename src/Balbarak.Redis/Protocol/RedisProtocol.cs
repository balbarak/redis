using Balbarak.Redis.Data;
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
        public Socket _socket;

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

        public async Task<bool> SetString(string key, string value)
        {
            var dataToSend = new RedisCommandBuilder("SET")
                .WithKey(key)
                .WithValue(value)
                .Build();

            var result = await SendCommandInternal(dataToSend);

            ValidateResult(result);

            return result.DataText == RedisResponse.OK;
        }

        public async Task<bool> SetBytes(string key, byte[] value)
        {
            var dataToSend = new RedisCommandBuilder("SET")
                .WithKey(key)
                .WithValue(value)
                .Build();

            var result = await SendCommandInternal(dataToSend);

            ValidateResult(result);

            return result.DataText == RedisResponse.OK;

        }

        public async Task<string> GetString(string key)
        {
            var dataToSend = new RedisCommandBuilder("GET")
                .WithKey(key)
                .Build();

            var result = await SendCommandInternal(dataToSend)
                .ConfigureAwait(false);

            ValidateResult(result);

            return result?.DataText; 
        }

        public async Task<byte[]> GetBytes(string key)
        {
            var dataToSend = new RedisCommandBuilder("GET")
                .WithKey(key)
                .Build();

            var result = await SendCommandInternal(dataToSend)
                .ConfigureAwait(false);

            ValidateResult(result);

            return result?.Data;
        }

        public async Task<bool> Exists(string key)
        {
            var dataToSend = new RedisCommandBuilder("EXISTS")
              .WithKey(key)
              .Build();

            var result = await SendCommandInternal(dataToSend);

            if (result.DataText == "0")
                return false;
            else
                return true;
        }

        public async Task<string> Ping()
        {
            var dataToSend = new RedisCommandBuilder("Ping").Build();

            var result = await SendCommandInternal(dataToSend);

            return result.DataText;
        }

        private async Task<RedisDataBlock> SendCommandInternal(byte[] data)
        {
            var sentBytes = await _socket.SendAsync(data, SocketFlags.None)
                .ConfigureAwait(false);

            var stream = new RedisStream(_socket);

            return await stream.ReadRedisData();
        }

        private void ValidateResult(RedisDataBlock result)
        {
            if (result == null)
                return;

            if (result.DataType == RedisDataType.Errors)
                throw new RedisException(result.DataText);
        }

    }
}
