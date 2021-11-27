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

        public async Task<string> Ping()
        {
            var dataToSend = $"PING \n".ToUTF8Bytes();

            var result = await SendCommandInternal(dataToSend);

            return ReadSimpleString(result);
        }

        public async Task<bool> Set(string key, string value)
        {
            var cmd = $"*3\r\n$3\r\nSET\r\n${key.Length}\r\n{key}\r\n${value.Length}\r\n{value}\r\n";
            //var cmd = $"SET {key} *1${value.Length}\r\n{value}\r\n";

            var data = cmd.ToUTF8Bytes();

            var redisRawData = await SendCommandInternal(data);

            ValidateError(redisRawData);

            var resultText = Encoding.UTF8.GetString(redisRawData);

            return resultText == RedisResponse.SUCCESS;
        }

        public async Task<byte[]> Get(string key)
        {
            var cmd = $"GET {key} \n";

            var data = cmd.ToUTF8Bytes();

            var rawData = await SendCommandInternal(data)
                .ConfigureAwait(false);

            ValidateError(rawData);

            var result = ReadData(rawData).ToArray();

            return result;
        }

        public async Task<string> GetBulkStrings(string key)
        {
            var dataToSend = $"GET {key} \n".ToUTF8Bytes();

            var result = await SendCommandInternal(dataToSend)
                .ConfigureAwait(false);

            if (result.Length == 0)
                throw new RedisException();


            return ReadBulkStrings(result);
        }

        public async Task<bool> Exists(string key)
        {
            var dataToSend = $"EXISTS {key} \n".ToUTF8Bytes();

            var redisRawData = await SendCommandInternal(dataToSend);

            var result = ReadInteger(redisRawData);

            if (result == 0)
                return false;
            else
                return true;
        }

        private string ReadBulkStrings(byte[] redisRawData)
        {
            var data = ReadData(redisRawData);

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

        private ReadOnlySpan<byte> ReadData(byte[] redisRawData)
        {
            var span = new ReadOnlySpan<byte>(redisRawData);

            var size = ReadDataSize(redisRawData, out int startIndex);

            return span.Slice(startIndex, size);
        }

        private int ReadDataSize(byte[] redisRawData, out int startIndex)
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

        private int ReadInteger(byte[] redisRawData)
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

            return await ReadRawData()
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
