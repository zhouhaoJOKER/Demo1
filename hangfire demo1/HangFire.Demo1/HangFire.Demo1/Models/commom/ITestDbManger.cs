using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace HangFire.Demo1.Models.commom
{
    public interface ITestDbManger
    {
        bool ExecuteNonQuery(string cmdtext);
        object ExecuteScalar(string cmdtext);

        DataTable FillData(string cmdtext);
    }
}
