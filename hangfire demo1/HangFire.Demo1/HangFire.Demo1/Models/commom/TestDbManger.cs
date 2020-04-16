using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace HangFire.Demo1.Models.commom
{
    public class TestDbManger : ITestDbManger
    {
        private readonly DefaultConnections DefaultConnections;

        public TestDbManger(IConfiguration configuration)
        {
            this.DefaultConnections = configuration.GetSection("DbConnections").Get<DefaultConnections>();
        }

        public bool ExecuteNonQuery(string cmdtext)
        {
            using (SqlConnection connection = new SqlConnection(this.DefaultConnections.localDb))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(cmdtext, connection);
                return command.ExecuteNonQuery() > 0;
            }
        }
        public object ExecuteScalar(string cmdtext)
        {
            using (SqlConnection connection = new SqlConnection(this.DefaultConnections.localDb))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(cmdtext, connection);
                return command.ExecuteScalar();
            }
        }

        public DataTable FillData(string cmdtext)
        {
            using (SqlConnection connection = new SqlConnection(this.DefaultConnections.localDb))
            {
                SqlCommand command = new SqlCommand(cmdtext, connection);
                command.CommandTimeout = 6000;
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }
    }
}
