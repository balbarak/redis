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

        public async Task<RedisDataBlock> ReadDataBlock()
        {
            RedisDataBlock result = null;

            var reader = PipeReader.Create(this);

            var readResult = await reader.ReadAsync();
            var buffer = readResult.Buffer;

            var firstByte = buffer.FirstSpan[0];

            //TODO: Remove duplicate allocation from RedisDataBlock
            if (firstByte == SIMPLE_STRINGS)
            {
                ProccessSimpleStringBlock(ref buffer, out var data);

                result = new RedisDataBlock(RedisDataType.SimpleStrings, buffer, data.Start,data.End);
            }

            if (firstByte == BULK_STRINGS)
            {
                var size = ReadSizeBlock(ref buffer, out var sizeData);

                if (size == -1)
                {
                    result = new RedisDataBlock(RedisDataType.EmptyBulkStrings, buffer, sizeData.Start,sizeData.End);
                }

                while (true && size > 0)
                {
                    if (buffer.Length > size)
                    {
                        var data = buffer.Slice(sizeData.Start, size);

                        result = new RedisDataBlock(RedisDataType.BulkStrings, buffer, data.Start, data.End);

                        break;
                    }

                    reader.AdvanceTo(buffer.Start, buffer.End);
                    readResult = await reader.ReadAsync();
                    buffer = readResult.Buffer;
                }
            }

            if (firstByte == ERRORS)
            {
                ProccessSimpleStringBlock(ref buffer, out var data);

                result = new RedisDataBlock(RedisDataType.Errors, buffer, data.Start,data.End);
            }

            if (firstByte == INTEGERS)
            {
                ProccessIntegersBlock(ref buffer, out var data);

                result = new RedisDataBlock(RedisDataType.Integers, buffer, data.Start, data.End);
            }

            await reader.CompleteAsync();

            return result;
        }

        private void ProccessSimpleStringBlock(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> data)
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

        private void ProccessIntegersBlock(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> data)
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
    }
}
