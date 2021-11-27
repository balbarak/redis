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

        }

        private void Setup()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
        }
    }
}
