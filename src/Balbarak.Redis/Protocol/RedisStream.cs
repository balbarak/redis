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
        public const int BUFFER_SIZE = 2048;

        private const byte BULK_STRINGS = (byte)'$';
        private const byte ARRAYS = (byte)'*';
        private const byte ERRORS = (byte)'-';
        private const byte SIMPLE_STRINGS = (byte)'+';
        private const byte INTEGERS = (byte)':';
        private const byte END = (byte)'\r';
        private static byte[] CLRF = new byte[2] { (byte)'\r', (byte)'\n' };

        public RedisStream(Socket socket) : base(socket)
        {

        }

        public async Task<byte[]> ReadRedisBuffer()
        {
            byte[] result = null;

            var reader = PipeReader.Create(this);
            var ms = new MemoryStream();

            var readResult = await reader.ReadAsync();
            var buffer = readResult.Buffer;

            var size = ReadSize(ref buffer);

            long totalBytesRead = 0;

            while (true)
            {
                var span = buffer.Slice(0, buffer.End);

                var str = Encoding.UTF8.GetString(span);

                totalBytesRead += buffer.Length;

                buffer = buffer.Slice(buffer.GetPosition(0, buffer.End));

                ms.Write(span.ToArray());

                if (totalBytesRead >= size)
                {
                    break;
                }

                reader.AdvanceTo(buffer.Start, buffer.End);
                readResult = await reader.ReadAsync();
                buffer = readResult.Buffer;
            }

            result = ms.ToArray();

            return result;
        }

        private RedisProtocolDataType GetDataType(byte firstByte)
        {
            if (firstByte == BULK_STRINGS)
                return RedisProtocolDataType.BulkStrings;

            if (firstByte == SIMPLE_STRINGS)
                return RedisProtocolDataType.SimpleStrings;

            return RedisProtocolDataType.Unkown;
        }

        private bool TryReadBulkString(ref ReadOnlySequence<byte> buffer, long totalSize, ref long totalBytesRead, out ReadOnlySequence<byte> dataToProccess)
        {
            if (totalBytesRead == totalSize)
            {
                //buffer = buffer.Slice(0, totalSize);

                dataToProccess = default;
                return false;
            }

            var bytesToRead = totalSize > buffer.Length ? buffer.Length : totalSize;

            dataToProccess = buffer.Slice(0,bytesToRead);

            buffer = buffer.Slice(bytesToRead);
            
            totalBytesRead += bytesToRead;

            return true;
        }

        private long ReadSize(ref ReadOnlySequence<byte> buffer)
        {
            SequencePosition? startPosition = buffer.PositionOf((byte)'$');
            SequencePosition? endPosition = buffer.PositionOf((byte)'\n');

            if (startPosition == null || endPosition == null)
            {
                return 0;
            }

            var sizeData = buffer.Slice(1, endPosition.Value);

            var sizeStr = Encoding.UTF8.GetString(sizeData);

            buffer = buffer.Slice(buffer.GetPosition(1, endPosition.Value));

            long.TryParse(sizeStr, out long result);


            return result;
        }

        
            
    }
}
