using Balbarak.Redis.Data;
using Balbarak.Redis.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
[assembly: InternalsVisibleToAttribute("Balbarak.Redis.Test")]
namespace Balbarak.Redis
{
    public class RedisClient : IRedisClient
    {
        private string _host;
        private int _prot;
        private RedisProtocol _protocol;

        public bool IsConnected { get; private set; }

        public event EventHandler OnConnected;

        public RedisClient(string host,int port)
        {
            _host = host;
            _prot = port;

            _protocol = new RedisProtocol();
        }

        public async Task Connect()
        {
            try
            {
                await _protocol.Connect(_host, _prot);

                IsConnected = true;
                OnConnected?.Invoke(this, default);
            }
            catch (Exception ex)
            {
                IsConnected = false;

                throw new RedisException($"Unable to connect to redis server {_host}:{_prot}", ex);
            }

        }

        public async Task<bool> Set(string key,string value)
        {
            if (!IsConnected)
                await Connect();

            var result = await _protocol.Set(key, value);

            ValidateResult(result);

            return result.DataText == RedisResponse.OK;
        }

        public async Task<string> GetStrings(string key)
        {
            if (!IsConnected)
                await Connect();

            var result = await _protocol.Get(key);

            ValidateResult(result);

            return result.DataText;
        }


        private void ValidateResult(RedisDataBlock result)
        {
            if (result.DataType == RedisDataType.Errors)
                throw new RedisException(result.DataText);
        }
    }
}
