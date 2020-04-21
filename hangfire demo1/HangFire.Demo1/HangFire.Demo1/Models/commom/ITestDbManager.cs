using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace HangFire.Demo1.Models.commom
{
    public interface ITestDbManager
    {
        bool ExecuteNonQuery(string cmdtext);
        object ExecuteScalar(string cmdtext);

        DataTable FillData(string cmdtext);
        object ExecuteScalarByParam(string cmdtext, SqlParameter[] param);

        bool SqlBulkCopy(DataTable sourceTable, string tableName);
    }
}
