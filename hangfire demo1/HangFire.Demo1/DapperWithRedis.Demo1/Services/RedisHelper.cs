using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DapperWithRedis.Demo1.Services
{
    public class RedisHelper : IRedisHelper
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<RedisHelper> logger;
        private readonly IDatabase database;

        public RedisHelper(IConfiguration configuration,ILogger<RedisHelper> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
            IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect("127.0.0.1,6379");
            connectionMultiplexer.ConnectionFailed += ConnectionFailed;
            database = connectionMultiplexer.GetDatabase();
        }
        public async Task<string> getRedisString(string key)
        {
            return await database.StringGetAsync(key);
        }
        public async Task<string> setRedisString(string key, string value, int seconds)
        {
            bool result = false;
            if (seconds == 0)
            {
                result = await database.StringSetAsync(key, value);
            }
            else 
            {
                result = await database.StringSetAsync(key, value, TimeSpan.FromSeconds(seconds));
            }
             
            if (result)
            {
                return value;
            }
            else
            {
                return "";
            }
        }

        public void ConnectionFailed(object s, ConnectionFailedEventArgs args) 
        {
            logger.LogError($"redis连接失败请查看 ConnectionType:{args.ConnectionType} \n failureType:{args.FailureType} ; EndPoint : {args.EndPoint.ToString()} ");
        }

        /// <summary>
        /// 新增或者是获取缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public async Task<string> GetOrAddString<T>(string key, Func<T> func, int seconds = 60)
        {
            this.logger.LogInformation($"getRedis {nameof(GetOrAddString)} key is {key}");
            string result = await getRedisString(key);
            if (string.IsNullOrEmpty(result))
            {
                if (func != null)
                {
                    T result_T = func();
                    string result_json = JsonConvert.SerializeObject(result_T);
                    this.logger.LogInformation($" getDb {nameof(GetOrAddString)} key is {key}");
                    await setRedisString(key, result_json, seconds);
                    result = result_json;
                }
                else 
                {
                    throw new ArgumentNullException(nameof(func));
                }
            }
            else 
            {
                return result;
            }
            return result;
        }
    }
}
