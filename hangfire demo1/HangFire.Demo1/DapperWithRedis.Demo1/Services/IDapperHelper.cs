using DapperWithRedis.Demo1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DapperWithRedis.Demo1.Services
{
    public interface IDapperHelper
    {
        /// <summary>
        /// 数据库中查找所有的商品属性
        /// </summary>
        /// <returns></returns>
        IEnumerable<Product> getAllProduct();

        /// <summary>
        /// 获取某一个商品
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        Product GetProduct(string productId);
    }
}
