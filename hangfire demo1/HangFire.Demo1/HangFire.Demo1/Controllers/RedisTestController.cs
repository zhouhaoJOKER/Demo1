using HangFire.Demo1.Models.commom;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HangFire.Demo1.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class RedisTestController : ControllerBase
    {
        private readonly RedisHelper redisHelpr;

        public RedisTestController(RedisHelper redisHelpr)
        {
            this.redisHelpr = redisHelpr;
        }
        public string Index() 
        {
            return "joker";
        }

        /// <summary>
        /// 设置一个值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet(nameof(SetString)+"/{name}")]
        public string SetString([FromRoute]string name) 
        {
            return redisHelpr.SetString(name);
        }

        [HttpGet(nameof(GetString)+"/{name}")]
        public string GetString([FromRoute]string name) 
        {
            string result = "";
            result = redisHelpr.GetString(name);
            return result;
        }
    }
}
