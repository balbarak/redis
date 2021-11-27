using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
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

        public const int BUFFER_SIZE = 2;

        public RedisStream(Socket socket) : base(socket)
        {

        }

        public async Task<byte[]> ReadRedisData()
        {
            byte[] result = null;

            //var reader = PipeReader.Create(this, new StreamPipeReaderOptions(bufferSize: 10));
            var reader = PipeReader.Create(this);

            while (true)
            {
                ReadResult readResult = await reader.ReadAsync();
                ReadOnlySequence<byte> buffer = readResult.Buffer;

                var fistByte = buffer.FirstSpan[0];

                if (fistByte == BULK_STRINGS)
                {
                    var size = GetDataSize(ref buffer,out SequencePosition? sizePos);

                    reader.AdvanceTo(sizePos.Value);

                    long totalReadBytes = 0;

                    while (totalReadBytes < size)
                    {
                        totalReadBytes += buffer.Length;

                        readResult = await reader.ReadAsync();
                        buffer = readResult.Buffer;

                        var strrr = Encoding.UTF8.GetString(buffer);

                        reader.AdvanceTo(buffer.Start,buffer.End);

                        if (totalReadBytes == size)
                            await reader.CompleteAsync();

                        Debug.WriteLine("Total bytes read: " + totalReadBytes);
                    }

                    if (readResult.IsCompleted)
                    {
                        break;
                    }
                }
            }

            return result;
        }

        private long GetDataSize(ref ReadOnlySequence<byte> buffer,out SequencePosition? position)
        {
            position = buffer.PositionOf((byte)'\r');

            var sizeBytes = buffer.Slice(1, position.Value);

            var sizeString = Encoding.UTF8.GetString(sizeBytes);

            long.TryParse(sizeString, out long result);

            return result;
        }

    }
}
