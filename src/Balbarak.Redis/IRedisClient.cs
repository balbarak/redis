using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis
{
    public interface IRedisClient
    {
        Task Connect();
        Task<T> Get<T>(string key);
        Task<byte[]> GetBytes(string key);
        Task<string> GetString(string key);
        Task<bool> Set(string key, string value);
        Task<bool> Set(string key, byte[] value);
        Task<bool> Set<T>(string key, T value);
    }
}
