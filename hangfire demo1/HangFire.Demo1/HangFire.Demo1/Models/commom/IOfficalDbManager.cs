using HangFire.Demo1.Models.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace HangFire.Demo1.Models.commom
{
    public interface IOfficalDbManager
    {
        bool ExecuteNonQuery(string cmdtext);
        object ExecuteScalar(string cmdtext);

        DataTable FillData(string cmdtext);
        IList<AuthorizationToken_QT> GetUserAccessToken_QT();
    }
}
