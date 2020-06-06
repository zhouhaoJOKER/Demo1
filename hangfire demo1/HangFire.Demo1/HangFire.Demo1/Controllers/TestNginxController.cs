using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HangFire.Demo1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestNginxController : ControllerBase
    {
        [HttpGet]
        public string Index() 
        {
            return "Frist Test ActionIndex";
        }
    }
}
