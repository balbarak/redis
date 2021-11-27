using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis.Protocol
{
    internal partial class RedisProtocol : RedisProtocolBase
    {
        //public const int BUFFER_SIZE = 8096;
        public const int BUFFER_SIZE = 2;

        public RedisProtocol()
        {

        }

        public async Task<string> Ping()
        {
            var dataToSend = $"PING \n".ToUTF8Bytes();

            var result = await SendCommandInternal(dataToSend);

            return ReadSimpleString(result);
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

            return resultText == RedisResponse.SUCCESS;
        }

        public async Task<byte[]> Get(string key)
        {
            var dataToSend = new RedisCommandBuilder("GET")
                .WithKey(key)
                .Build();

            var rawData = await SendCommandInternal(dataToSend)
                .ConfigureAwait(false);

            ValidateError(rawData);

            var result = ReadData(ref rawData).ToArray();

            return result;
        }

        public async Task<string> GetBulkStrings(string key)
        {
            var dataToSend = new RedisCommandBuilder("GET")
               .WithKey(key)
               .Build();

            var result = await SendCommandInternal(dataToSend)
                .ConfigureAwait(false);

            if (result.Length == 0)
                throw new RedisException();


            return ReadBulkStrings(ref result);
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

        private string ReadBulkStrings(ref byte[] redisRawData)
        {
            var data = ReadData(ref redisRawData);

            return Encoding.UTF8.GetString(data);
        }

        private string ReadSimpleString(byte[] redisRawData)
        {
            var span = new ReadOnlySpan<byte>(redisRawData);

            if (redisRawData == null)
                return null;

            if (redisRawData[0] != (byte)'+')
                return null;

            var textBytes = span.Slice(1, redisRawData.Length - 3);

            return Encoding.UTF8.GetString(textBytes);
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
            var sentBytes = await _socket.SendAsync(data, SocketFlags.None)
                .ConfigureAwait(false);

            return await ReadStreamRawData()
                .ConfigureAwait(false);
        }

        private async Task<byte[]> ReadRawData()
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

        private async Task<byte[]> ReadStreamRawData()
        {
            byte[] result = null;

            long totalBytesRead = 0;

            var stream = new RedisStream(_socket);

            await stream.ReadRedisData();

            //var buffer = new byte[BUFFER_SIZE];

            //var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

            //if (bytesRead == 0)
            //    return null;

            //totalBytesRead += bytesRead;

            //using (MemoryStream ms = new MemoryStream())
            //{
            //    var isBulkStrings = buffer[0] == RedisProtocolDataTypes.BULK_STRINGS;

            //    if (isBulkStrings)
            //    {
            //        var size = ReadDataSize(ref buffer,out int startIndex);
            //    }
            //}

            //var data = buffer.Skip(0).Take(bytesRead).ToArray();

            //result.AddRange(data);

            //while (!IsEndOfData(data))
            //{
            //    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

            //    data = buffer.Skip(0).Take(bytesRead).ToArray();

            //    result.AddRange(data);
            //}

            return result;

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
