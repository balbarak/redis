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
        private string _password;
        private RedisProtocol _protocol;
        private RedisConfiguration _config;

        public bool IsConnected { get; private set; }

        public event EventHandler OnConnected;

        public RedisClient(string host,int port)
        {
            _protocol = new RedisProtocol();
            _config = new RedisConfiguration(host, port);
        }

        public RedisClient(string host, int port,string password) : this(host,port) 
        {
            _password = password;
        }

        public async Task Connect()
        {
            try
            {
                await _protocol.Connect(_config.Host, _config.Port);

                if (!string.IsNullOrWhiteSpace(_password))
                {
                    var authResult = await _protocol.Auth(_password);

                    ValidateResult(authResult);
                }
              
                IsConnected = true;
                OnConnected?.Invoke(this, default);
            }
            catch (Exception ex)
            {
                IsConnected = false;

                throw new RedisException($"Unable to connect to redis server {_config.Host}:{_config.Port}", ex);
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

        public async Task<string> GetStrings(string key)
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

        private void ValidateResult(RedisDataBlock result)
        {
            if (result.Type == RedisDataType.Errors)
                throw new RedisException(result.Result);
        }
    }
}
