using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFire.Demo1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestDistributeCacheController : ControllerBase
    {
        private readonly IDistributedCache distributedCache;

        public TestDistributeCacheController(IDistributedCache distributedCache)
        {
            this.distributedCache = distributedCache;
        }
        [HttpGet]
        public async Task<string> getName() 
        {
            var result = "";
            var bys = await this.distributedCache.GetAsync("name").ConfigureAwait(false);
            if (bys != null)
            {
                result = Encoding.UTF8.GetString(bys);
                return result;
            }
            else 
            {
                bys = Encoding.UTF8.GetBytes("joker");
                var option = new DistributedCacheEntryOptions()
                {
                    SlidingExpiration = TimeSpan.FromSeconds(30)
                };
                await this.distributedCache.SetAsync("name",bys, option);
                return "joker";
            }
        }
    }
}
