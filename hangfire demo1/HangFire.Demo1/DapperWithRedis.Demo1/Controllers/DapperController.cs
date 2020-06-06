using DapperWithRedis.Demo1.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DapperWithRedis.Demo1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DapperController : ControllerBase
    {
        private readonly IProductManager productManager;

        public DapperController(IProductManager productManager)
        {
            this.productManager = productManager;
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> getAll() 
        {
            var products = await productManager.getAllProduct();
            return JsonConvert.SerializeObject(products);
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<string> getProduct([FromRoute]string id)
        {
            var product = await productManager.findProduct(id);
            return JsonConvert.SerializeObject(product);
        }
    }
}
