using HangFire.Demo1.Models.commom;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace HangFire.Demo1.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ITestDbManger testDbManger;

        public TestController(ITestDbManger testDbManger)
        {
            this.testDbManger = testDbManger;
        }

        public string Index() 
        {
            string sql = "select top 1 spdm from shangpin";
            string spdm = testDbManger.ExecuteScalar(sql).ToString();
            return spdm;
        }

        [HttpGet("getInfo")]
        public string getInfo() 
        {
            string sql = "select top 100 spdm,spmc from shangpin";
            var spdms = testDbManger.FillData(sql);
            return JsonConvert.SerializeObject(spdms);
        }
    }
}
