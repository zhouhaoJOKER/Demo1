using DapperWithRedis.Demo1.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DapperWithRedis.Demo1.Services
{
    public class ProductManager : IProductManager
    {
        private readonly IDapperHelper dapperHelper;
        private readonly IRedisHelper redisHelper;

        public ProductManager(IDapperHelper dapperHelper,IRedisHelper redisHelper)
        {
            this.dapperHelper = dapperHelper;
            this.redisHelper = redisHelper;
        }
        public async Task<Product> findProduct(string id)
        {
            string redisKey = $"Product_{id}";
            Product product = null;
            string result = await redisHelper.GetOrAddString<Product>(redisKey, () => 
            {
                return dapperHelper.GetProduct(id);
            }, 60);
            if (!string.IsNullOrWhiteSpace(result)) 
            {
                product = JsonConvert.DeserializeObject<Product>(result);
            }
            return await Task.FromResult(product);
        } 
        public async Task<IEnumerable<Product>> getAllProduct()
        {
            string key = $"Product_AllProduct";
            IEnumerable<Product> products = dapperHelper.getAllProduct();
            string result = await redisHelper.GetOrAddString<IEnumerable<Product>>(key, () => 
            {
                return dapperHelper.getAllProduct();
            }, 0);

            return await Task.FromResult(products);
        } 

    }
}
