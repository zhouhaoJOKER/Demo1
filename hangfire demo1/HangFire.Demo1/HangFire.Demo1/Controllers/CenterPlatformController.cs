using HangFire.Demo1.Models.commom;
using HangFire.Demo1.Models.ERPConnetionForQT.set;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HangFire.Demo1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CenterPlatformController : ControllerBase
    {
        private readonly IOfficalDbManager officalDbManager;
        private readonly ITestDbManager testDbManager;
        private readonly ConfigUtil configUtil;

        public CenterPlatformController(
            IOfficalDbManager officalDbManager,
            ITestDbManager testDbManager,
            ConfigUtil configUtil)
        {
            this.officalDbManager = officalDbManager;
            this.testDbManager = testDbManager;
            this.configUtil = configUtil;
        }

        [HttpGet]
        public string index() 
        {
            SetSearchOnlineGoodsInfo ss = new SetSearchOnlineGoodsInfo(officalDbManager, testDbManager, configUtil);
            ss.Run();
            return "";
        }
    }
}
