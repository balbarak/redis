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
        private RedisProtocol _protocol;
        private RedisConfiguration _config;

        public RedisConfiguration Settings => _config;

        public bool IsConnected { get; private set; }

        public event EventHandler OnConnected;

        public RedisClient(string host,int port)
        {
            _protocol = new RedisProtocol();
            _config = new RedisConfiguration(host, port);
        }

        public RedisClient(string host, int port,string password) : this(host,port) 
        {
           _config.Password = password; ;
        }

        public RedisClient(RedisConfiguration config)
        {
            _protocol = new RedisProtocol();
            _config = config;
        }

        public async Task Connect()
        {
            try
            {
                await _protocol.Connect(_config.Host, _config.Port);

                await Authenticate();

                IsConnected = true;
                OnConnected?.Invoke(this, default);
            }
            catch (Exception ex)
            {
                IsConnected = false;

                throw new RedisException($"Unable to connect to redis server {_config.Host}:{_config.Port}. See inner exception for more details", ex);
            }

        }

        public async Task<bool> Set(string key,string value)
        {
            if (!IsConnected)
                await Connect();

            var result = await _protocol.Set(key, value);

            ValidateResult(result);

            return result.Result == RedisResponse.OK;
        }

        public async Task<bool> Set(string key,byte[] value)
        {
            if (!IsConnected)
                await Connect();

            var result = await _protocol.Set(key, value);

            ValidateResult(result);

            return result.Result == RedisResponse.OK;
        }

        public async Task<bool> Set<T>(string key, T value)
        {
            if (!IsConnected)
                await Connect();

            var data = _config.Serializer.Serialize(value);

            var result = await _protocol.Set(key, data);

            ValidateResult(result);

            return result.Result == RedisResponse.OK;
        }

        public async Task<T> Get<T>(string key)
        {
            if (!IsConnected)
                await Connect();

            var data = await GetBytes(key);

            return _config.Serializer.Deserialize<T>(data);
        }

        public async Task<string> GetString(string key)
        {
            if (!IsConnected)
                await Connect();

            var result = await _protocol.Get(key);

            ValidateResult(result);

            return result.Result;
        }

        public async Task<byte[]> GetBytes(string key)
        {
            if (!IsConnected)
                await Connect();

            var result = await _protocol.Get(key);

            ValidateResult(result);

            return result.ResultData;
        }

        public async Task<int> Delete(params string[] keys)
        {
            if (!IsConnected)
                await Connect();

            var result = await _protocol.Del(keys);

            ValidateResult(result);

            return int.Parse(result.Result);
        }

        private async Task Authenticate()
        {
            if (!string.IsNullOrWhiteSpace(_config.Password))
            {
                var authResult = await _protocol.Auth(_config.Password);

                ValidateResult(authResult);
            }
        }

        private void ValidateResult(RedisDataBlock result)
        {
            if (result.Type == RedisDataType.Errors)
                throw new RedisException(result.Result);
        }
    }
}
