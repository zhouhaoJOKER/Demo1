using log4net.Core;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HangFire.Demo1.Filters
{
    public class TestApiActionFilterAttribute : ActionFilterAttribute
    {
        private readonly ILogger<TestApiActionFilterAttribute> logger;

        public TestApiActionFilterAttribute(ILogger<TestApiActionFilterAttribute> logger)
        {
            this.logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            IDictionary<string, string> dic = new Dictionary<string, string>();
            foreach (var item in context.HttpContext.Request.RouteValues) 
            {
                if (!dic.ContainsKey(item.Key)) 
                {
                    dic.Add(item.Key, item.Value.ToString());
                }
            }

            logger.LogInformation("ActionExecutingContext:" + JsonConvert.SerializeObject(dic));
        }
    }
}
