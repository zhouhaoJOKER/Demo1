using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace HangFire.Demo1.Models.commom
{
    public class BusinessDbUtil
    {
        public static void DoAction(Action<DirectDbManger> action)
        {
            DirectDbManger dbManager = new DirectDbManger();
            action(dbManager);
        }

        public static DataTable GetDataTable(string sql)
        {
            DataTable table = new DataTable();
            DirectDbManger dbManager = new DirectDbManger();
            return dbManager.FillData(sql);
        }

        public static bool ExecuteNonQuery(string sql)
        {
            DirectDbManger dbManager = new DirectDbManger();
            return dbManager.ExecuteNonQuery(sql);
        }

        public DataTable ExecProcedure(string procName, DataTable dtParms)
        {
            DirectDbManger dbManager = new DirectDbManger();
            return dbManager.ExecProcedure(procName, dtParms);
        }

        public static string ExecuteBatchNonQuery(string[] sqlList)
        {
            DirectDbManger dbManager = new DirectDbManger();
            return dbManager.ExecuteBatchQuery(sqlList);
        }

        public static string ExecuteBatchNonQuery(out bool flag, string masterSql, string[] sqlList)
        {
            DirectDbManger dbManager = new DirectDbManger();
            return dbManager.ExecuteBatchQuery(out flag, masterSql, sqlList);
        }

        public static object ExecuteScalar(string sql)
        {
            DirectDbManger dbManager = new DirectDbManger();
            return dbManager.ExecuteScalar(sql);
        }

        internal static string ExecuteBillId(string sql)
        {
            var result = ExecuteScalar(sql);
            if (result == null || result == DBNull.Value)
            {
                return "";
            }
            return (string)result;
        }

        public static int ExecuteScalarInt(string sql)
        {
            var result = ExecuteScalar(sql);
            if (result == null)
            {
                return -1;
            }

            if (result is int)
            {
                return (int)result;
            }

            if (result is byte)
            {
                return (byte)result;
            }
            if (result is Int16)
            {
                return (Int16)result;
            }

            int oInt;
            if (int.TryParse(result.ToString(), out oInt))
            {
                return oInt;
            }

            return -1;
        }

        /// <summary>
        /// 执行sql;返回影响行数
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static int ExecuteNonQueryInt(string sql)
        {
            DirectDbManger dbManager = new DirectDbManger();
            return dbManager.ExecuteNonQueryInt(sql);
        }
    }

    public class DirectDbManger
    {
        public DirectDbManger()
        {
        }

        public bool ExecuteNonQuery(string cmdtext)
        {
            using (SqlConnection connection = new SqlConnection(ConfigUtil.ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(cmdtext, connection);
                return command.ExecuteNonQuery() > 0;
            }
        }

        public DataTable ExecProcedure(string procName, DataTable dtParms)
        {
            DataTable table = new DataTable();
            var selectCommand = new SqlCommand();
            try
            {
                DataRow[] rowArray;
                SqlParameter[] parameters = GetParameters(procName);
                selectCommand.CommandType = CommandType.StoredProcedure;
                selectCommand.CommandText = procName;
                if (dtParms != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        rowArray = dtParms.Select("POSITION=" + ((i + 1)).ToString());
                        if (rowArray.Length > 0)
                        {
                            DbType dbType = parameters[i].DbType;
                            parameters[i].Value = rowArray[0]["PARMVALUE"];
                        }
                    }
                }
                if (parameters != null)
                {
                    foreach (SqlParameter parameter in parameters)
                    {
                        selectCommand.Parameters.Add(parameter);
                    }
                }
                selectCommand.Connection = new SqlConnection(ConfigUtil.ConnectionString);
                new SqlDataAdapter(selectCommand).Fill(table);
                foreach (SqlParameter parameter in selectCommand.Parameters)
                {
                    if (parameter.Direction == ParameterDirection.Output)
                    {
                        rowArray = dtParms.Select("ARGUMENT_NAME='" + parameter.ParameterName + "'");
                        if (rowArray.Length > 0)
                        {
                            rowArray[0]["PARMVALUE"] = parameter.Value;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                LogUtil.Write(ex.ToString());
            }
            finally
            {
                selectCommand.Dispose();
            }
            return table;
        }

        private SqlParameter[] GetParameters(string ProcName)
        {
            SqlParameter[] parameterArray = null;
            DataTable procedureParameter = GetProcedureParameter(ProcName);
            if (procedureParameter.Rows.Count != 0)
            {
                parameterArray = new SqlParameter[procedureParameter.Rows.Count];
                for (int i = 0; i < procedureParameter.Rows.Count; i++)
                {
                    parameterArray[i] = new SqlParameter
                    {
                        ParameterName = procedureParameter.Rows[i]["ARGUMENT_NAME"].ToString(),
                        SqlDbType = DataType(procedureParameter.Rows[i]["DATA_TYPE"].ToString()),
                        Direction = (procedureParameter.Rows[i]["IN_OUT"].ToString() == "IN")
                                        ? ParameterDirection.Input
                                        : ParameterDirection.InputOutput
                    };
                }
            }
            return parameterArray;
        }

        private SqlDbType DataType(string DATA_TYPE)
        {
            SqlDbType bigInt = SqlDbType.BigInt;
            switch (DATA_TYPE.ToUpper())
            {
                case "BIGINT":
                    return SqlDbType.BigInt;

                case "BINARY":
                    return SqlDbType.Binary;

                case "BIT":
                    return SqlDbType.Bit;

                case "CHAR":
                    return SqlDbType.Char;

                case "DATETIME":
                    return SqlDbType.DateTime;

                case "DECIMAL":
                    return SqlDbType.Decimal;

                case "FLOAT":
                    return SqlDbType.Float;

                case "IMAGE":
                    return SqlDbType.Image;

                case "INT":
                    return SqlDbType.Int;

                case "MONEY":
                    return SqlDbType.Money;

                case "NCHAR":
                    return SqlDbType.NChar;

                case "NTEXT":
                    return SqlDbType.NText;

                case "NVARCHAR":
                    return SqlDbType.NVarChar;

                case "REAL":
                    return SqlDbType.Real;

                case "SMALLDATETIME":
                    return SqlDbType.SmallDateTime;

                case "SMALLINT":
                    return SqlDbType.SmallInt;

                case "SMALLMONEY":
                    return SqlDbType.SmallMoney;

                case "TEXT":
                    return SqlDbType.Text;

                case "TIMESTAMP":
                    return SqlDbType.Timestamp;

                case "TINYINT":
                    return SqlDbType.TinyInt;

                case "UDT":
                    return SqlDbType.Udt;

                case "UNIQUEIDENTIFIER":
                    return SqlDbType.UniqueIdentifier;

                case "VARBINARY":
                    return SqlDbType.VarBinary;

                case "VARCHAR":
                    return SqlDbType.VarChar;

                case "VARIANT":
                    return SqlDbType.Variant;

                case "XML":
                    return SqlDbType.Xml;
            }
            return bigInt;
        }

        public DataTable GetProcedureParameter(string procName)
        {
            string format =
                "select Specific_Name as OBJECT_NAME,ORDINAL_POSITION as POSITION,PARAMETER_NAME as ARGUMENT_NAME ,DATA_TYPE ,PARAMETER_MODE as IN_OUT,CHARACTER_MAXIMUM_LENGTH as DATA_LENGTH,NUMERIC_PRECISION as DATA_PRECISION,NUMERIC_SCALE as DATA_SCALE  from INFORMATION_SCHEMA.PARAMETERS where Specific_Name='{0}' ORDER BY POSITION ";
            format = string.Format(format, procName, procName);
            DataTable table = FillData(format);
            table.Columns.Add("PARMVALUE");
            return table;
        }

        public void SetProcedureParameter(DataTable parameters, string parameterName, object parameterValue)
        {
            bool flag = false;
            for (int i = 0; i < parameters.Rows.Count; i++)
            {
                DataRow row = parameters.Rows[i];
                if (((row["ARGUMENT_NAME"].ToString().ToLower().Trim() == parameterName.ToLower().Trim()) || (("@" + row["ARGUMENT_NAME"].ToString().ToLower().Trim()) == parameterName.ToLower().Trim())) || (row["ARGUMENT_NAME"].ToString().ToLower().Trim() == ("@" + parameterName.ToLower().Trim())))
                {
                    flag = true;
                    row["PARMVALUE"] = parameterValue;
                }
            }
            if (!flag)
            {
                throw new Exception("存储过程未找到参数:" + parameterName);
            }
        }

        public string ExecuteBatchQuery(IEnumerable<string> sqlList)
        {
            using (SqlConnection connection = new SqlConnection(ConfigUtil.ConnectionString))
            {
                connection.Open();
                SqlTransaction trans = connection.BeginTransaction();
                string sql = string.Empty;
                try
                {
                    foreach (string sqlItem in sqlList)
                    {
                        sql = sqlItem;
                        SqlCommand command = new SqlCommand(sql, connection, trans);
                        command.ExecuteNonQuery();
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    LogUtil.Write(ex.ToString());
                    trans.Rollback();
                    return sql;
                }
            }
            return string.Empty;
        }

        public string ExecuteBatchQuery(out bool flag, string masterSql, IEnumerable<string> sqlList)
        {
            flag = false;
            using (SqlConnection connection = new SqlConnection(ConfigUtil.ConnectionString))
            {
                connection.Open();
                SqlTransaction trans = connection.BeginTransaction();
                string sql = string.Empty;
                try
                {
                    SqlCommand command = new SqlCommand(masterSql, connection, trans);
                    command.ExecuteNonQuery();

                    foreach (string sqlItem in sqlList)
                    {
                        sql = sqlItem;
                        command = new SqlCommand(sql, connection, trans);
                        command.ExecuteNonQuery();
                    }
                    trans.Commit();
                    flag = true;
                }
                catch (Exception ex)
                {
                    LogUtil.Write(ex.ToString());
                    trans.Rollback();
                    return sql;
                }
            }
            return string.Empty;
        }

        public string ExecuteBatchQuery(out bool flag, string masterSql, IEnumerable<string> sqlList, IEnumerable<string> sqlList2)
        {
            flag = false;
            using (SqlConnection connection = new SqlConnection(ConfigUtil.ConnectionString))
            {
                connection.Open();
                SqlTransaction trans = connection.BeginTransaction();
                string sql = string.Empty;
                try
                {
                    SqlCommand command = new SqlCommand(masterSql, connection, trans);
                    command.ExecuteNonQuery();

                    foreach (string sqlItem in sqlList)
                    {
                        sql = sqlItem;
                        command = new SqlCommand(sql, connection, trans);
                        command.ExecuteNonQuery();
                    }

                    foreach (string sqlItem2 in sqlList2)
                    {
                        sql = sqlItem2;
                        command = new SqlCommand(sql, connection, trans);
                        command.ExecuteNonQuery();
                    }

                    trans.Commit();
                    flag = true;
                }
                catch (Exception ex)
                {
                    LogUtil.Write(ex.ToString());
                    trans.Rollback();
                    return sql;
                }
            }
            return string.Empty;
        }

        public string ExecuteWXTDBatchQuery(out bool flag, string djbh, string masterSql, string sql2, IEnumerable<string> sqlList)
        {
            StringBuilder sb = new StringBuilder();
            flag = false;
            using (SqlConnection connection = new SqlConnection(ConfigUtil.ConnectionString))
            {
                connection.Open();
                SqlTransaction trans = connection.BeginTransaction();
                string sql = string.Empty;
                try
                {
                    sb.Append(masterSql);
                    SqlCommand command = new SqlCommand(masterSql, connection, trans);
                    command.ExecuteNonQuery();

                    sb.Append("\r\n");
                    sb.Append(sql2);
                    SqlCommand command2 = new SqlCommand(sql2, connection, trans);
                    command2.ExecuteNonQuery();

                    foreach (string sqlItem in sqlList)
                    {
                        sql = sqlItem;
                        sb.Append("\r\n");
                        sb.Append(sql);
                        command = new SqlCommand(sql, connection, trans);
                        command.ExecuteNonQuery();
                    }

                    sql = string.Format("UPDATE WXTD SET je=(SELECT SUM(JE) FROM dbo.WXTDSPMX WHERE DJBH='{0}'),sl=(SELECT SUM(SL) FROM dbo.WXTDSPMX WHERE DJBH='{0}') WHERE dbo.WXTD.DJBH = '{0}'", djbh);
                    command = new SqlCommand(sql, connection, trans);
                    command.ExecuteNonQuery();

                    trans.Commit();
                    flag = true;
                }
                catch (Exception ex)
                {
                    LogUtil.Write(sb + ex.ToString());
                    trans.Rollback();
                    return sql;
                }
            }
            return string.Empty;
        }


        public string ExecuteBatchQuery(out bool flag, string masterSql, string sql2, IEnumerable<string> sqlList)
        {
            StringBuilder sb = new StringBuilder();
            flag = false;
            using (SqlConnection connection = new SqlConnection(ConfigUtil.ConnectionString))
            {
                connection.Open();
                SqlTransaction trans = connection.BeginTransaction();
                string sql = string.Empty;
                try
                {
                    sb.Append(masterSql);
                    SqlCommand command = new SqlCommand(masterSql, connection, trans);
                    command.ExecuteNonQuery();

                    sb.Append("\r\n");
                    sb.Append(sql2);
                    SqlCommand command2 = new SqlCommand(sql2, connection, trans);
                    command2.ExecuteNonQuery();

                    foreach (string sqlItem in sqlList)
                    {
                        sql = sqlItem;
                        sb.Append("\r\n");
                        sb.Append(sql);
                        command = new SqlCommand(sql, connection, trans);
                        command.ExecuteNonQuery();
                    }
                    trans.Commit();
                    flag = true;
                }
                catch (Exception ex)
                {
                    LogUtil.Write(sb + ex.ToString());
                    trans.Rollback();
                    return sql;
                }
            }
            return string.Empty;
        }


        public string ExecuteWXDDBatchQuery(out bool flag, string djbh, string masterSql, string sql2, string sql3, IEnumerable<string> sqlList)
        {
            StringBuilder sb = new StringBuilder();
            flag = false;
            using (SqlConnection connection = new SqlConnection(ConfigUtil.ConnectionString))
            {
                connection.Open();
                SqlTransaction trans = connection.BeginTransaction();
                string sql = string.Empty;
                try
                {
                    sb.Append(masterSql);
                    SqlCommand command = new SqlCommand(masterSql, connection, trans);
                    command.ExecuteNonQuery();

                    sb.Append("\r\n");
                    sb.Append(sql2);
                    command = new SqlCommand(sql2, connection, trans);
                    command.ExecuteNonQuery();

                    sb.Append("\r\n");
                    sb.Append(sql3);
                    command = new SqlCommand(sql3, connection, trans);
                    command.ExecuteNonQuery();

                    foreach (string sqlItem in sqlList)
                    {
                        sql = sqlItem;
                        sb.Append("\r\n");
                        sb.Append(sql);
                        command = new SqlCommand(sql, connection, trans);
                        command.ExecuteNonQuery();
                    }

                    sql = string.Format("UPDATE WXDDJEMX SET je=(SELECT SUM(JE) FROM dbo.WXDDSPMX WHERE DJBH='{0}'), ddje=(SELECT SUM(JE) FROM dbo.WXDDSPMX WHERE DJBH='{0}')," +
                                         "sl=(SELECT SUM(SL) FROM dbo.WXDDSPMX WHERE DJBH='{0}'),spjtje=(SELECT SUM(ddyfje) FROM dbo.WXDDJEMX WHERE DJBH='{0}') WHERE dbo.WXDDJEMX.DJBH = '{0}'", djbh);
                    command = new SqlCommand(sql, connection, trans);
                    command.ExecuteNonQuery();

                    sql = string.Format("UPDATE WXDD SET je=(SELECT SUM(JE) FROM dbo.WXDDSPMX WHERE DJBH='{0}'),sl=(SELECT SUM(SL) FROM dbo.WXDDSPMX WHERE DJBH='{0}') WHERE dbo.WXDD.DJBH = '{0}'", djbh);
                    command = new SqlCommand(sql, connection, trans);
                    command.ExecuteNonQuery();

                    trans.Commit();
                    flag = true;
                }
                catch (Exception ex)
                {
                    LogUtil.Write(sb + ex.ToString());
                    trans.Rollback();
                    return sql;
                }
            }
            return string.Empty;
        }

        public int ExecuteNonQueryInt(string cmdtext)
        {
            using (SqlConnection connection = new SqlConnection(ConfigUtil.ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(cmdtext, connection);
                return command.ExecuteNonQuery();
            }
        }

        public object ExecuteScalar(string cmdtext)
        {
            using (SqlConnection connection = new SqlConnection(ConfigUtil.ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(cmdtext, connection);
                return command.ExecuteScalar();
            }
        }

        public DataTable FillData(string cmdtext)
        {
            using (SqlConnection connection = new SqlConnection(ConfigUtil.ConnectionString))
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
