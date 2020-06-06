using System;
using System.Threading.Tasks;

namespace DapperWithRedis.Demo1.Services
{
    public interface IRedisHelper
    {
        Task<string> GetOrAddString<T>(string key, Func<T> func, int seconds = 60);
        Task<string> getRedisString(string key);
        Task<string> setRedisString(string key, string value, int seconds);
    }
}