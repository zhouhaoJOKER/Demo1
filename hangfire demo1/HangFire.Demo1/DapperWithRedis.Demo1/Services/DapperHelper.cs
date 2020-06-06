using DapperWithRedis.Demo1.Models;
using Microsoft.Extensions.Configuration;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace DapperWithRedis.Demo1.Services
{
    public class DapperHelper : IDapperHelper
    {
        private readonly IConfiguration configuration;
        private readonly string defaultConnectionString ;

        public DapperHelper(IConfiguration configuration)
        {
            this.configuration = configuration;
            defaultConnectionString = configuration.GetConnectionString("DefaultString") ?? 
                                       throw new ArgumentNullException(nameof(defaultConnectionString))  ;  
        }

        public IEnumerable<Product> getAllProduct()
        {
            string sql = "select top 100 spdm,spmc,zjf from shangpin";
            IEnumerable<Product> products = null;
            using (IDbConnection db = new SqlConnection(defaultConnectionString)) 
            {
                products = db.Query<Product>(sql);
            }
            return products;
        }

        public Product GetProduct(string productId)
        {
            string sql = "select top 100 spdm,spmc,zjf from shangpin where spdm = @spdm";
            IEnumerable<Product> products = null;
            using (IDbConnection db = new SqlConnection(defaultConnectionString))
            {
                products = db.Query<Product>(sql,new { spdm = productId });
            }
            return products.FirstOrDefault();
        }
    }
}
