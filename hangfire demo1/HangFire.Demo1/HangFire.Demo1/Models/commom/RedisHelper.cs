using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HangFire.Demo1.Models.commom
{
    public class RedisHelper
    {
        private readonly IDatabase redisDb;

        public RedisHelper(IConfiguration configuration)
        {
            var RedisConnections = configuration.GetSection("RedisConnections").Get<RedisConnections>();
            var redis = ConnectionMultiplexer.Connect(RedisConnections.DefaultRedisConnection);
            this.redisDb = redis.GetDatabase(RedisConnections.Default);
        }

        public string SetString(string message) 
        {
            this.redisDb.StringSet("message",message);
            return message;
        }

        public string GetString(string message)
        {
            return this.redisDb.StringGet("message");
        }
    }
}
