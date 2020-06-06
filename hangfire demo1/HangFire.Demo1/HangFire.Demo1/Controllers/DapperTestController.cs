using HangFire.Demo1.DBHelpers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HangFire.Demo1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DapperTestController :ControllerBase
    {
        private readonly DBHelper DBHelper;

        public DapperTestController(DBHelper DBHelper)
        {
            this.DBHelper = DBHelper;
        }
        [HttpGet]
        public string index() 
        {
            DBHelper.DapperTestMethod_One("lq01");
            return "OK";
        }
    }
}
