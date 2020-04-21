using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.OleDb;
using HangFire.Demo1.Models.commom;

namespace HangFire.Demo1.Models.CenterPlatform
{
    public class StoredProc
    {
        private string _procName = string.Empty;
        private readonly ITestDbManager testDbManager;
        private Dictionary<string, SqlParameter> dicSqlParams = new Dictionary<string, SqlParameter>(); 

        public StoredProc(string procName, ITestDbManager testDbManager)
        {
            this._procName = procName;
            this.testDbManager = testDbManager;
        } 
        public void AddParameter(string parameterName, SqlDbType dbType, int size, ParameterDirection direction, object value)
        {
            SqlParameter param = new SqlParameter(parameterName, dbType, size);
            param.Direction = direction;
            param.Value = value;
            if (!dicSqlParams.ContainsKey(parameterName))
            {
                dicSqlParams.Add(parameterName, param);
            }

        } 
        public void AddParameter(string parameterName, SqlDbType dbType, ParameterDirection direction)
        {
            SqlParameter param = new SqlParameter(parameterName, dbType);
            param.Direction = direction;
            if (!dicSqlParams.ContainsKey(parameterName))
            {
                dicSqlParams.Add(parameterName, param);
            }
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <returns></returns>
        public bool Execute()
        {
            bool result = false;
            SqlParameter[] arrSqlParam = dicSqlParams.Values.ToArray<SqlParameter>();
            object obj = testDbManager.ExecuteScalarByParam(_procName, arrSqlParam);
            if (obj != null)
            {
                result = Convert.ToInt32(obj) == 1;
            }
            return result;
        }
        public string INFExecute()
        {
            string result = string.Empty;
            SqlParameter[] arrSqlParam = dicSqlParams.Values.ToArray<SqlParameter>();
            object obj = testDbManager.ExecuteScalarByParam(_procName, arrSqlParam);
            if (obj != null)
            {
                result = obj.ToString();
            }
            return result;
        }
        public SqlParameter[] getSqlParameter()
        {
            SqlParameter[] arrSqlParam = dicSqlParams.Values.ToArray<SqlParameter>();
            return arrSqlParam;
        }
    }
}
