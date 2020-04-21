using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace HangFire.Demo1.Models.commom
{
    public class TestDbManager : ITestDbManager
    {
        private readonly DefaultConnections DefaultConnections;

        public TestDbManager(IConfiguration configuration)
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

        /// <summary>
        /// 建立数据库命令执行对象 
        /// </summary>
        /// <param name="strCmd">要执行的SQL语句</param>
        /// <param name="conn">数据库连接对象</param>
        /// <returns>数据库命令对象</returns>
        public IDbCommand CreateCommand(string strCmd, IDbConnection conn)
        {
            IDbCommand cmd = null;
            cmd = new SqlCommand(strCmd, (SqlConnection)conn);
            cmd.CommandTimeout = 7200;
            return cmd;
        }

        public object ExecuteScalarByParam(string strCmd, SqlParameter[] paras)
        {
            IDbConnection connection = null;
            connection =  new SqlConnection(DefaultConnections.localDb.ToString());
            object result = null;
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            try
            {
                SqlParameter temp = null;
                using (IDbCommand command = CreateCommand(strCmd, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    foreach (SqlParameter param in paras)
                    {
                        if (param.Direction == ParameterDirection.Output)
                        {
                            temp = param;
                        }
                        command.Parameters.Add(param);
                    }
                    command.ExecuteNonQuery();
                }
                result = temp.Value.ToString();
            }
            catch (Exception ScalarException)
            {
                connection.Close();
                throw ScalarException;
            }
            finally
            {
                connection.Close();
            }

            return result;
        }

        /// <summary>
        /// <summary>
        /// 批量插入数据
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <param name="tableName">表明</param>
        /// <returns></returns>
        public bool SqlBulkCopy(DataTable sourceTable, string tableName)
        {
            bool result = false;
            IDbConnection connection = null;
            connection = new SqlConnection(DefaultConnections.localDb.ToString());

            if (connection.State == ConnectionState.Closed)
                connection.Open();
            IDbTransaction transaction = connection.BeginTransaction();
            try
            {
                using (SqlBulkCopy bulkCoye = new SqlBulkCopy((SqlConnection)connection, SqlBulkCopyOptions.Default, (SqlTransaction)transaction))
                {
                    bulkCoye.DestinationTableName = tableName;
                    ////设定超时时间  
                    bulkCoye.BulkCopyTimeout = 60;
                    ////每批插入的行数(如果入库失败，只是本批次事物回滚，如果想全部回滚还需要加参数SqlTransaction)  
                    bulkCoye.BatchSize = 1000;
                    ////在上面定义的批次里，每准备插入1条数据时，呼叫相应的事件（这时只是准备，没有真正入库）  
                    bulkCoye.NotifyAfter = 1000;
                    bulkCoye.WriteToServer(sourceTable);
                    transaction.Commit();
                    bulkCoye.Close();
                }
                result = true;
            }
            catch (Exception e)
            {
                transaction.Rollback();
                throw e;
            }
            finally
            {
                connection.Close();
            }

            return result;
        }
    }
}
