using HangFire.Demo1.Models.CenterPlatform;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFire.Demo1.Models.commom
{
    public  class InvoicesManage
    {
        private static ITestDbManager _testDbManager;

        public InvoicesManage(ITestDbManager testDbManager)
        {
            _testDbManager = testDbManager;
        }
        /// <summary>
        /// 取仓库库位
        /// </summary>
        /// <param name="KWDM"></param>
        /// <returns></returns>
        public static string GetCKKW(string CKDM)
        {

            var sql = string.Empty;
            sql = string.Format("select top 1 KWDM from CKKW where CKDM='{0}' AND ISNULL(BYZD2,0)=1 ", CKDM);//默认库位
            var data = _testDbManager.ExecuteScalar(sql);
            if (data != null)
            {
                return data.ToString();
            }
            else
            {
                sql = string.Format("select top 1 KWDM from CKKW where CKDM='{0}' ", CKDM);
                var data1 = _testDbManager.ExecuteScalar(sql);
                if (data1 != null)
                {
                    return data1.ToString();
                }
                else
                {
                    return "";
                }

            }
        }

        /// <summary>
        /// 获取价格选定值
        /// </summary>
        /// <param name="JGSD"></param>
        /// <returns></returns>
        public static int GetJGSD(string JGSD)
        {
            int _JGSD = 0;
            switch (JGSD)
            {

                case "BZSJ":
                    {
                        _JGSD = 3;
                        break;
                    }
                case "SJ1":
                    {
                        _JGSD = 4;
                        break;
                    }
                case "SJ2":
                    {
                        _JGSD = 5;
                        break;
                    }
                case "SJ3":
                    {
                        _JGSD = 6;
                        break;
                    }
                case "SJ4":
                    {
                        _JGSD = 7;
                        break;
                    }
                case "BZJJ":
                    {
                        _JGSD = 0;
                        break;
                    }
                case "JJ1":
                    {
                        _JGSD = 1;
                        break;
                    }
                case "JJ2":
                    {
                        _JGSD = 2;
                        break;
                    }

            }
            return _JGSD;
        }
        /// <summary>
        /// 取仓库表中信息
        /// </summary>
        /// <param name="Fild">字段</param>
        /// <param name="TableName">表</param>
        /// <returns></returns>
        public static string GetCANGKUValue(string Fild, string CKDM)
        {

            string sql = string.Format("select {0} from CANGKU where CKDM='{1}'", Fild, CKDM);
            var data = _testDbManager.ExecuteScalar(sql);
            if (data != null)
            {
                return data.ToString();
            }
            else
            {
                return "";
            }
        }
        /// <summary>
        /// 生成单据编号
        /// </summary>
        /// <param name="_TableName"></param>
        /// <param name="YDJH"></param>
        /// <param name="_vFuncID"></param>
        /// <returns></returns>
        public static string GetNewDJBH(string _TableName, string QDDM, string KHDM, string YDJH)
        {
            var DJBH = string.Empty;
            DJBH = InvoicesManage.ProduceDJBH(QDDM, KHDM, _TableName);
            if (string.IsNullOrEmpty(DJBH))
            {
                DJBH = YDJH;
            }
            return DJBH;
        }
        public static string ProduceDJBH(string QDDM, string KHDM, string TableName)
        {
            var DJBH = string.Empty;
            #region//类型
            var Type = 1;
            switch (TableName)
            {
                case "LSXHD":
                    {
                        Type = 5;
                        break;
                    }
                case "LSTHD":
                    {
                        Type = 5;
                        break;
                    }
                case "SDPHD":
                    {
                        Type = 3;
                        break;
                    }
                case "PSEND":
                    {
                        Type = 3;
                        break;
                    }
                case "PTSND":
                    {
                        Type = 3;
                        break;
                    }
                case "SDTHD":
                    {
                        Type = 3;
                        break;
                    }
                case "PFXHD":
                    {
                        Type = 4;
                        break;
                    }
                case "PFTHD":
                    {
                        Type = 4;
                        break;
                    }
                case "FSEND":
                    {
                        Type = 4;
                        break;
                    }
                case "FTSND":
                    {
                        Type = 4;
                        break;
                    }
                case "CKTZD":
                    {
                        Type = 6;
                        break;
                    }
                case "SPJHD":
                    {
                        Type = 1;
                        break;
                    }
                case "SPTHD":
                    {
                        Type = 1;
                        break;
                    }
                case "SPYCD":
                    {
                        Type = 6;
                        break;
                    }
                case "QDDBD":
                    {
                        Type = 2;
                        break;
                    }
                case "DSEND":
                    {
                        Type = 2;
                        break;
                    }
                case "DTSND":
                    {
                        Type = 2;
                        break;
                    }
                case "QDTHD":
                    {
                        Type = 2;
                        break;
                    }
                case "SPPKD":
                    {
                        Type = 1;
                        break;
                    }
                case "QDNPD":
                    {
                        Type = 7;
                        break;
                    }
                case "SPTPD":
                    {
                        Type = 7;
                        break;
                    }
                case "JSEND":
                    {
                        Type = 1;
                        break;
                    }
            }
            #endregion
            DJBH = GetNewDJBH(Type, QDDM, KHDM, TableName);
            return DJBH;
        }
        public static string GetNewDJBH(int Type, string QDDM, string KHDM, string TableName)
        {
            try
            {
                string pic = "P_GET_BILL_CODE_New";
                string DJBH = string.Empty;
                if (string.IsNullOrEmpty(QDDM))
                {
                    QDDM = "000";
                }  
                StoredProc storeProc = new StoredProc(pic, _testDbManager);
                storeProc.AddParameter("@TYPE", SqlDbType.Int, 4, ParameterDirection.Input, Type);
                storeProc.AddParameter("@CODE0", SqlDbType.VarChar, 50, ParameterDirection.Input, "");
                storeProc.AddParameter("@CODE1", SqlDbType.VarChar, 50, ParameterDirection.Input, DateTime.Now.ToString("yyyyMMdd"));
                storeProc.AddParameter("@CODE2", SqlDbType.VarChar, 50, ParameterDirection.Input, QDDM);
                storeProc.AddParameter("@CODE3", SqlDbType.VarChar, 50, ParameterDirection.Input, KHDM);
                storeProc.AddParameter("@CODE4", SqlDbType.VarChar, 50, ParameterDirection.Input, "");
                storeProc.AddParameter("@TABLE", SqlDbType.VarChar, 50, ParameterDirection.Input, TableName);
                storeProc.AddParameter("@BUFFER", SqlDbType.VarChar, 20, ParameterDirection.Output, DJBH);
                DJBH = storeProc.INFExecute();
                return DJBH;
                // P_GET_BILL_CODE(3, ' ', v_NOW, v_QDDM, v_KHDM, ' ', 'SDPHD', v_BILL) ;
            }
            catch (Exception ex)
            { 
                return "";
            }
        }

        public static DataTable SetDataTable(Dictionary<string, object> list, string TableName)
        {
            DataTable pdt = new DataTable(TableName);
            foreach (var item in list)
            {
                pdt.Columns.Add(item.Key, typeof(string));
            }
            DataRow dr = pdt.NewRow();
            foreach (var item in list)
            {
                dr[item.Key] = item.Value;
            }
            pdt.Rows.Add(dr);
            return pdt;
        }

        public static string SetSQLValue(Dictionary<string, object> list, string tablename, string djbh, string ObjectName)
        {
            var sql = string.Empty;
            List<string> SQLLIST = new List<string>();
            foreach (var item in list)
            {
                SQLLIST.Add(item.Key);
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");
            for (int i = 0; i < SQLLIST.Count; i++)
            {

                sb.AppendFormat(SQLLIST[i]);
                if (i < SQLLIST.Count - 1)
                {
                    sb.Append(",");
                }
            }
            sb.AppendFormat(" FROM {0} ", tablename);
            sb.AppendFormat(" WHERE 1=2 ");
            return sb.ToString();
        }
    }
}
