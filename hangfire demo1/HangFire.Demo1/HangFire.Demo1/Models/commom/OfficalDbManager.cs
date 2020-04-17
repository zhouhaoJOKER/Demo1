using HangFire.Demo1.Models.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HangFire.Demo1.Models.commom
{
    public class OfficalDbManager : IOfficalDbManager
    {
        private readonly DefaultConnections DefaultConnections;
        public OfficalDbManager(IConfiguration configuration)
        {
            this.DefaultConnections = configuration.GetSection("DbConnections").Get<DefaultConnections>();
        }
        public bool ExecuteNonQuery(string cmdtext)
        {
            using (SqlConnection connection = new SqlConnection(this.DefaultConnections.officalDb))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(cmdtext, connection);
                return command.ExecuteNonQuery() > 0;
            }
        }
        public object ExecuteScalar(string cmdtext)
        {
            using (SqlConnection connection = new SqlConnection(this.DefaultConnections.officalDb))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(cmdtext, connection);
                return command.ExecuteScalar();
            }
        } 
        public DataTable FillData(string cmdtext)
        {
            using (SqlConnection connection = new SqlConnection(this.DefaultConnections.officalDb))
            {
                SqlCommand command = new SqlCommand(cmdtext, connection);
                command.CommandTimeout = 6000;
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        /// <summary>
        /// 获取授权
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public IList<AuthorizationToken_QT> GetUserAccessToken_QT()
        {
            string sql = "SELECT access_token,taobao_user_nick FROM AuthorizationToken_QT WHERE ISNULL(taobao_user_nick,'')<>'' GROUP BY access_token,taobao_user_nick";
            DataTable table = new DataTable(); 
            using (var connection = new SqlConnection(this.DefaultConnections.officalDb))
            {
                System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, connection);
                command.CommandTimeout = 6000;
                System.Data.SqlClient.SqlDataAdapter adapter = new System.Data.SqlClient.SqlDataAdapter(command);
                adapter.Fill(table);
            }
            if (table != null && table.Rows.Count > 0)
            {
                string json_DataTable = JsonConvert.SerializeObject(table);
                return JsonConvert.DeserializeObject<List<AuthorizationToken_QT>>(json_DataTable);
            }
            else 
            {
                return null;
            } 
        }
    }
}
