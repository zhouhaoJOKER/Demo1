using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HangFire.Demo1.Services
{
    public class TestModel : IBillType
    {
        public void WriteInfo(string message) 
        {
            LogUtil.WriteInfo(this, message, message);
        }
        public string BillType => nameof(TestModel);
    }
}
