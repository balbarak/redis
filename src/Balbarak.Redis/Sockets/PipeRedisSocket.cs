using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Balbarak.Redis.Test")]
namespace Balbarak.Redis.Sockets
{
    
    internal class PipeRedisSocket : RedisSocketBase
    {
        public override async Task<byte[]> SendData(byte[] data)
        {
            var sentBytes = await _socket.SendAsync(data, SocketFlags.None);

            return await ReadInternal();
        }
        
        private async Task<byte[]> ReadInternal()
        {
            var stream = new NetworkStream(_socket);
            var reader = PipeReader.Create(stream);
            var bytesResult = new List<byte>();

            while (true)
            {
                ReadResult result = await reader.ReadAsync();
                ReadOnlySequence<byte> buffer = result.Buffer;

                while (TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
                {
                    //try to process data without allocate !
                    //ProcessLine(line);

                    var data = ReadData(line);
                    bytesResult.AddRange(data);
                }

                await reader.CompleteAsync();

                break;
            }

            return bytesResult.ToArray();
        }

        private bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
        {
            // Look for a EOL in the buffer.
            SequencePosition? position = buffer.PositionOf((byte)'\n');

            if (position == null)
            {
                line = default;
                return false;
            }

            // Skip the line + the \n.
            line = buffer.Slice(0, position.Value);
            buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
            return true;
        }

        private void ProcessLine(in ReadOnlySequence<byte> buffer)
        {
            foreach (var segment in buffer)
            {
                Debug.Write(Encoding.UTF8.GetString(segment.Span));
            }

            Debug.WriteLine(" ");
        }

        private byte[] ReadData(in ReadOnlySequence<byte> buffer)
        {
            var result = new List<byte>();

            foreach (var segment in buffer)
            {
                result.AddRange(segment.Span.ToArray());
            }

            return result.ToArray();
        }

    }
}
