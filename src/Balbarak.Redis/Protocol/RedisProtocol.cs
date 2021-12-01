﻿using Balbarak.Redis.Data;
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
        internal Socket _socket;

        public RedisProtocol()
        {
            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        public async Task Connect(string host, int port)
        {
            await _socket.ConnectAsync(host, port)
                .ConfigureAwait(false);
        }

        public async Task<RedisDataBlock> SetString(string key, string value)
        {
            var dataToSend = new RedisCommandBuilder("SET")
                .WithKey(key)
                .WithValue(value)
                .Build();

            return await SendCommandInternal(dataToSend)
                .ConfigureAwait(false);
        }

        public async Task<RedisDataBlock> SetBytes(string key, byte[] value)
        {
            var dataToSend = new RedisCommandBuilder("SET")
                .WithKey(key)
                .WithValue(value)
                .Build();

            return await SendCommandInternal(dataToSend)
                .ConfigureAwait(false);
        }

        public async Task<RedisDataBlock> GetString(string key)
        {
            var dataToSend = new RedisCommandBuilder("GET")
                .WithKey(key)
                .Build();

            return await SendCommandInternal(dataToSend)
                .ConfigureAwait(false);

        }

        public async Task<RedisDataBlock> GetBytes(string key)
        {
            var dataToSend = new RedisCommandBuilder("GET")
                .WithKey(key)
                .Build();

            return await SendCommandInternal(dataToSend)
                .ConfigureAwait(false);
        }

        public async Task<RedisDataBlock> Exists(string key)
        {
            var dataToSend = new RedisCommandBuilder("EXISTS")
              .WithKey(key)
              .Build();

            return await SendCommandInternal(dataToSend)
                .ConfigureAwait(false);
        }

        public async Task<RedisDataBlock> Ping()
        {
            var dataToSend = new RedisCommandBuilder("Ping").Build();

            return  await SendCommandInternal(dataToSend)
                .ConfigureAwait(false);
        }

        private async Task<RedisDataBlock> SendCommandInternal(byte[] data)
        {
            var sentBytes = await _socket.SendAsync(data, SocketFlags.None)
                .ConfigureAwait(false);

            var stream = new RedisStream(_socket);

            return await stream.ReadRedisData()
                .ConfigureAwait(false);
        }

    }
}
