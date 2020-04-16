using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace HangFire.Demo1.Models.commom
{
    public class InvoicesManage
    {
        public static DataTable GetUserAccessToken_QT(string sql)
        {
            DataTable table = new DataTable();
            DirectDbManger dbManager = new DirectDbManger();
            using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection("USER ID=sa;PASSWORD=Bs201409;INITIAL CATALOG=BSERP3_ZB_1130CJ;DATA SOURCE=120.24.63.214,1433;CONNECT TIMEOUT=30"))
            {
                System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, connection);
                command.CommandTimeout = 6000;
                System.Data.SqlClient.SqlDataAdapter adapter = new System.Data.SqlClient.SqlDataAdapter(command);
                adapter.Fill(table);
            }
            return table;
        }
    }
}
