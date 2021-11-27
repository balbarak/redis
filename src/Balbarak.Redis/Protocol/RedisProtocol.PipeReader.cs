using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis.Protocol
{
    internal partial class RedisProtocol
    {
        public async Task<byte[]> SendCommandWithPipeReader(byte[] data)
        {
            var sentBytes = await _socket.SendAsync(data, SocketFlags.None)
                .ConfigureAwait(false);

            return await ReadRawDataWithPipeReader()
                .ConfigureAwait(false);
        }

        private async Task<byte[]> ReadRawDataWithPipeReader()
        {
            var stream = new NetworkStream(_socket);
            var reader = PipeReader.Create(stream);
            var bytesResult = new List<byte>();

            while (true)
            {
                ReadResult result = await reader.ReadAsync();
                ReadOnlySequence<byte> buffer = result.Buffer;

                var dataType = GetDataType(ref buffer);

                switch (dataType)
                {
                    case RedisProtocolDataType.Integers:
                        break;
                    case RedisProtocolDataType.BulkStrings:

                        while (TryReadBulkStringSegment(ref buffer, out ReadOnlySequence<byte> segment))
                        {

                            
                        }

                        break;
                    case RedisProtocolDataType.Arrays:

                        while (TryReadSegment(ref buffer, out ReadOnlySequence<byte> segment))
                        {

                            reader.AdvanceTo(buffer.Start, buffer.End);
                        }
                        break;
                    case RedisProtocolDataType.SimpleStrings:
                        break;
                    case RedisProtocolDataType.Errors:
                        break;
                    case RedisProtocolDataType.Unkown:
                        break;
                    default:
                        break;

                }

                reader.AdvanceTo(buffer.Start, buffer.End);

                if (result.IsCompleted)
                {
                    break;
                }


                //while (TryReadSegment(ref buffer, out ReadOnlySequence<byte> line))
                //{
                //    //try to process data without allocate !
                //    //ProcessLine(line);

                //    //var data = ReadData(line);
                //    //bytesResult.AddRange(data);
                //}

                await reader.CompleteAsync();

                break;
            }

            return bytesResult.ToArray();
        }

        private bool TryReadSegment(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> segment)
        {
            // Look for a EOL in the buffer.
            var position = buffer.PositionOf(RedisProtocolDataTypes.BULK_STRINGS);

            var bulkStringPosition = buffer.PositionOf(RedisProtocolDataTypes.BULK_STRINGS);

            if (bulkStringPosition != null)
            {
                var dataSizePosition = buffer.PositionOf((byte)'\r');

            }

            if (position == null)
            {
                segment = default;
                return false;
            }

            // Skip the line + the \n.
            segment = buffer.Slice(0, position.Value);

            var str = Encoding.UTF8.GetString(segment);

            buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
            return true;
        }

        private bool TryReadBulkStringSegment(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> segment)
        {
            var position = buffer.PositionOf((byte)'\r');

            segment = default;

            if (position == null)
            {
                segment = default;
                //return false;
            }

            var sizeSegment = buffer.Slice(1, position.Value);


            //// Skip the line + the \n.
            //segment = buffer.Slice(0, position.Value);
            //var str = Encoding.UTF8.GetString(sizeSegment);
            buffer = buffer.Slice(buffer.GetPosition(1, position.Value));

            return true;
        }

        private RedisProtocolDataType GetDataType(ref ReadOnlySequence<byte> buffer)
        {
            var pos = buffer.PositionOf(RedisProtocolDataTypes.BULK_STRINGS);

            if (pos != null)
                return RedisProtocolDataType.BulkStrings;

            return RedisProtocolDataType.Unkown;
        }
    }
}
