using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis
{
    public class RedisClient
    {
        private int BUFFER_SIZE = 8096;

        private Socket _socket;
        private string _host;
        private int _prot;
        private NetworkStream _stream;

        public bool IsConnected { get; private set; }

        public event EventHandler OnConnected;


        public RedisClient(string host,int port)
        {
            _host = host;
            _prot = port;

            Setup();
        }

        public async Task Connect()
        {
            await _socket.ConnectAsync(_host, _prot);

            _stream = new NetworkStream(_socket);

        }

        public async Task Ping()
        {
            var cmd = new RedisCommand("get FIX\n", "");
            var data = cmd.GetBytesToSend();


            var bytesSend = await _socket.SendAsync(data, SocketFlags.None)
                .ConfigureAwait(false);

            var buffer = new byte[BUFFER_SIZE];

            var memory = new Memory<byte>(buffer);

            var bytesRead = await _socket.ReceiveAsync(memory, SocketFlags.None)
                .ConfigureAwait(false);

            var fff = buffer.Skip(0).Take(bytesRead).ToArray();

            var str = Encoding.UTF8.GetString(fff);

        }

        private void Setup()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
        }
    }
}
