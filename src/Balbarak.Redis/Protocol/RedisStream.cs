using Balbarak.Redis.Data;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis.Protocol
{
    internal class RedisStream : NetworkStream
    {
        private const byte BULK_STRINGS = (byte)'$';
        private const byte ARRAYS = (byte)'*';
        private const byte ERRORS = (byte)'-';
        private const byte SIMPLE_STRINGS = (byte)'+';
        private const byte INTEGERS = (byte)':';
        private const byte END = (byte)'\r';
        private const byte NEW_LINE = (byte)'\n';
        private static byte[] CLRF = new byte[2] { (byte)'\r', (byte)'\n' };

        public RedisStream(Socket socket) : base(socket)
        {

        }

        public async Task<byte[]> ReadRedisBuffer()
        {
            var ms = new MemoryStream();
            var reader = PipeReader.Create(this);

            var readResult = await reader.ReadAsync();
            var buffer = readResult.Buffer;

            var firstByte = buffer.FirstSpan[0];

            if (firstByte == SIMPLE_STRINGS)
            {
                var data = ProccessSimpleString(ref buffer);

                return data.ToArray();
            }

            if (firstByte == BULK_STRINGS)
            {
                var size = ReadSize(ref buffer);

                while (true && size > 0)
                {
                    if (buffer.Length >= size)
                    {
                        var span = buffer.Slice(0, buffer.Length - 2);

                        ms.Write(span.ToArray());

                        break;
                    }

                    reader.AdvanceTo(buffer.Start, buffer.End);
                    readResult = await reader.ReadAsync();
                    buffer = readResult.Buffer;
                }

                return ms.ToArray();
            }

            if (firstByte == ERRORS)
            {
                var data = ProccessSimpleString(ref buffer);

                return data.ToArray();
            }

            if (firstByte == INTEGERS)
            {
                var data = ProccessIntegers(ref buffer);

                return data.ToArray();
            }

            await reader.CompleteAsync();

            return null;
        }

        public async Task<RedisDataBlock> ReadRedisData()
        {
            var reader = PipeReader.Create(this);

            var readResult = await reader.ReadAsync();
            var buffer = readResult.Buffer;

            var bufferStr = Encoding.UTF8.GetString(buffer);

            var firstByte = buffer.FirstSpan[0];

            if (firstByte == SIMPLE_STRINGS)
            {
                ProccessSimpleString(ref buffer, out var data);

                return new RedisDataBlock(RedisDataType.SimpleStrings, buffer.ToArray(), data.ToArray());
            }

            if (firstByte == BULK_STRINGS)
            {
                var size = ReadSizeBlock(ref buffer, out var sizeData);

                while (true)
                {
                    if (buffer.Length >= size)
                    {
                        var rawData = buffer.ToArray();

                        var endPosition = buffer.Length > size ? buffer.Length : size;

                        var data = buffer.Slice(sizeData.Start, endPosition).ToArray();

                        return new RedisDataBlock(RedisDataType.BulkStrings,rawData,data);
                    }

                    reader.AdvanceTo(buffer.Start, buffer.End);
                    readResult = await reader.ReadAsync();
                    buffer = readResult.Buffer;
                }
            }
            await reader.CompleteAsync();

            return null;
        }

        private byte[] ProccessSimpleString(ref ReadOnlySequence<byte> buffer)
        {
            var endPosition = buffer.PositionOf(END);
            var newLine = buffer.PositionOf(NEW_LINE);

            if (endPosition == null || newLine == null)
                return null;

            var result = buffer.Slice(1, endPosition.Value).ToArray();

            buffer.Slice(1, newLine.Value);

            return result;
        }

        private void ProccessSimpleString(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> data)
        {
            var endPosition = buffer.PositionOf(END);
            var newLine = buffer.PositionOf(NEW_LINE);

            if (endPosition == null || newLine == null)
            {
                data = default;
                return;
            }

            data = buffer.Slice(1, endPosition.Value);
        }

        private byte[] ProccessIntegers(ref ReadOnlySequence<byte> buffer)
        {
            var endPosition = buffer.PositionOf(END);
            var newLine = buffer.PositionOf(NEW_LINE);

            if (endPosition == null || newLine == null)
                return null;

            var result = buffer.Slice(1, endPosition.Value).ToArray();

            buffer.Slice(1, newLine.Value);

            return result;
        }

        private long ReadSize(ref ReadOnlySequence<byte> buffer)
        {
            var startPosition = buffer.PositionOf((byte)'$');
            var endPosition = buffer.PositionOf((byte)'\r');
            var dataEnd = buffer.PositionOf((byte)'\n');

            if (startPosition == null || endPosition == null || dataEnd == null)
            {
                return 0;
            }

            var sizeData = buffer.Slice(1, endPosition.Value);

            var sizeStr = Encoding.UTF8.GetString(sizeData);

            buffer = buffer.Slice(buffer.GetPosition(1, dataEnd.Value));

            long.TryParse(sizeStr, out long result);

            return result;
        }

        private long ReadSizeBlock(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> data)
        {
            var startPosition = buffer.PositionOf((byte)'$');
            var endPosition = buffer.PositionOf((byte)'\r');
            var finalPosition = buffer.PositionOf((byte)'\n');

            if (startPosition == null || endPosition == null || finalPosition == null)
            {
                data = default;
                return 0;
            }

            var sizeData = buffer.Slice(1, endPosition.Value);

            var sizeStr = Encoding.UTF8.GetString(sizeData);

            data = buffer.Slice(buffer.GetPosition(1, finalPosition.Value));

            long.TryParse(sizeStr, out long result);

            return result;
        }
    }
}
