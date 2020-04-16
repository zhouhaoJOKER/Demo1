using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using HangFire.Demo1.Services;

namespace HangFire.Demo1.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IBackgroundJobClient jobClient;
        private readonly IRecurringJobManager recurringJobManager;

        public HomeController(IBackgroundJobClient jobClient
            ,IRecurringJobManager recurringJobManager)
        {
            this.jobClient = jobClient;
            this.recurringJobManager = recurringJobManager;
        }
        [HttpGet(nameof(getName))]
        public async Task<string> getName() 
        {
            return await Task.Run(() => nameof(getName)).ConfigureAwait(false) ;        
        }

        [HttpGet(nameof(AddBackGroundJob)+ "/{jobDescription}")]
        public void AddBackGroundJob(string jobDescription) 
        {
            this.jobClient.Enqueue(()=> Console.WriteLine(jobDescription));
        }

        [HttpGet(nameof(AddBackGroundJob_Two) + "/{jobDescription}")]
        public void AddBackGroundJob_Two(string jobDescription)
        {
            var test = new TestModel();
            this.jobClient.Enqueue(() => test.WriteInfo(jobDescription));
        }

        [HttpGet(nameof(AddRecurringJob)+"/{content}")]
        public void AddRecurringJob(string content) 
        {
            var test = new TestModel();
            recurringJobManager.AddOrUpdate("PrintJob", () => test.WriteInfo(content),Cron.Minutely);
        }
        [HttpGet(nameof(RemoveRecurringJob) + "/{jobId}")]
        public void RemoveRecurringJob(string jobId) 
        {
            recurringJobManager.RemoveIfExists(jobId);
        }

        [HttpGet(nameof(TriggerJob) + "/{jobId}")]
        public void TriggerJob(string jobId)
        {
            recurringJobManager.Trigger(jobId);
        }
    }
}
