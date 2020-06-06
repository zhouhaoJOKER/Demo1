using DapperWithRedis.Demo1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DapperWithRedis.Demo1.Services
{
    /// <summary>
    /// 定义行为
    /// </summary>
    public interface IProductManager
    {
        /// <summary>
        /// 获取所有的商品信息
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Product>> getAllProduct();

        /// <summary>
        /// 获取某一个商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Product> findProduct(string id);
    }
}
