using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HangFire.Demo1.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class LoggerTestController : ControllerBase
    {
        private readonly ILogger<LoggerTestController> logger;

        public LoggerTestController(ILogger<LoggerTestController> logger)
        {
            this.logger = logger;
        }
        public void Index() 
        {
            this.logger.LogInformation("joker");
        }
    }
}
