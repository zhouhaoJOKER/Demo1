using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HangFire.Demo1.Filters
{
    /// <summary>
    /// 定义在action的filter
    /// </summary>
    public class TestTwoActionFilterAttribute : Attribute, IActionFilter, IFilterMetadata
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            Console.WriteLine($"{nameof(TestTwoActionFilterAttribute)}OnActionExecuting执行之前---定义在action的filter");
            //base.OnActionExecuting(context);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            Console.WriteLine($"{nameof(TestTwoActionFilterAttribute)}OnResultExecuted执行之后---定义在action的filter");
        }
    }

    /// <summary>
    /// 定义在全局的filter
    /// </summary>
    public class TestGlobalActionFilterAttribute : Attribute, IActionFilter, IFilterMetadata
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            Console.WriteLine($"{nameof(TestGlobalActionFilterAttribute)}OnActionExecuted执行之前---定义在全局的filter");
            
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            Console.WriteLine($"{nameof(TestGlobalActionFilterAttribute)}OnActionExecuting执行之前---定义在全局的filter");
        }
    }

    /// <summary>
    /// 定义在controller的filter
    /// </summary>
    public class TestControllerActionFilterAttribute : Attribute, IActionFilter, IFilterMetadata
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            Console.WriteLine($"{nameof(TestControllerActionFilterAttribute)}OnActionExecuted执行之前---定义在controller的filter");
            //throw new NotImplementedException();
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            Console.WriteLine($"{nameof(TestControllerActionFilterAttribute)}OnActionExecuting执行之前---定义在controller的filter");
        }
    }
}
