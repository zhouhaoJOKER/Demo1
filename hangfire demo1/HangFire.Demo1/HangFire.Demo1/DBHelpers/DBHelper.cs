using HangFire.Demo1.Models.commom;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HangFire.Demo1.DBHelpers
{
    public class DBHelper
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<DBHelper> logger;
        private readonly string connectionString;
        

        public DBHelper(IConfiguration configuration,ILogger<DBHelper> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
            connectionString = configuration.GetSection("DbConnections").Get<DefaultConnections>().localDb 
                ?? throw new ArgumentNullException(nameof(connectionString)); 
        }
        public void DapperTestMethod_One(string spdm) 
        {
            using (IDbConnection db = new SqlConnection(connectionString)) 
            {
                string sql = "select top 100 spdm,spmc from shangpin where spdm = '"+spdm+"'";
                IEnumerable<dynamic> dynamics = db.Query(sql);
                var item = dynamics.ToList()[0]?.spmc;
                logger.LogInformation("Dapper 输出的数据 " + JsonConvert.SerializeObject(dynamics));
            } 
        }
    }
}
