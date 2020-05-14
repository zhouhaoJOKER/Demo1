using HangFire.Demo1.Models.commom;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using HangFire.Demo1.Filters;

namespace HangFire.Demo1.Controllers
{
    //[TestControllerActionFilter]
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ITestDbManager testDbManager;
        private readonly IMemoryCache memoryCache;
        private readonly IOfficalDbManager officalDbManager;

        public TestController(ITestDbManager testDbManager,
            IMemoryCache memoryCache,
            IOfficalDbManager officalDbManager)
        {
            this.testDbManager = testDbManager;
            this.memoryCache = memoryCache;
            this.officalDbManager = officalDbManager;
        }

        //这个方式的话 是为了注入其他的服务比如 Ilogger
        [ServiceFilter(typeof(TestApiActionFilterAttribute), IsReusable = true)]
        [HttpGet("getspmc/{spdm}")]
        public string getspmc([FromRoute]string spdm) 
        {
            if (string.IsNullOrEmpty(spdm)) 
            {
                throw new ArgumentNullException(nameof(spdm));
            }
            string MyKey = $"SPDM:{spdm}";
            string spmc = "";
            
            if (!memoryCache.TryGetValue(MyKey, out spmc))
            {
                // Key not in cache, so get data.
                string sql = $"select top 1 spmc from shangpin where spdm = '{spdm}'";
                spmc = testDbManager.ExecuteScalar(sql).ToString();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    // Set cache entry size by extension method.
                    .SetSize(1)
                    // Keep in cache for this time, reset time if accessed.
                    //.SetSlidingExpiration(TimeSpan.FromSeconds(30))
                    //相对的时间 过了时间就清除
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
                    
                // Set cache entry size via property.
                // cacheEntryOptions.Size = 1; 
                // Save data in cache.
                memoryCache.Set(MyKey, spmc, cacheEntryOptions);
            }   
            return spmc;
        }

        [TestTwoActionFilter]
        [HttpGet("getInfo")]
        public string getInfo() 
        {
            Console.WriteLine("执行方法");
            string sql = "select top 100 spdm,spmc from shangpin";
            var spdms = testDbManager.FillData(sql);
            return JsonConvert.SerializeObject(spdms);
        }

        /// <summary>
        /// 获取授权
        /// </summary>
        /// <returns></returns>
        [HttpGet("getAuth")]
        public string getAuth() 
        {
            string AuthKeys = "AuthKeys";
            if (!memoryCache.TryGetValue(AuthKeys, out string AuthValues)) 
            {
                var lists = officalDbManager.GetUserAccessToken_QT();
                AuthValues = JsonConvert.SerializeObject(lists);
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    // Set cache entry size by extension method.
                    .SetSize(1)
                    // Keep in cache for this time, reset time if accessed.
                    //.SetSlidingExpiration(TimeSpan.FromSeconds(30))
                    //相对的时间 过了时间就清除
                    .SetAbsoluteExpiration(TimeSpan.FromDays(365));
                memoryCache.Set(AuthKeys, AuthValues, cacheEntryOptions); 
            } 
            return AuthValues;
        }
    }
}
