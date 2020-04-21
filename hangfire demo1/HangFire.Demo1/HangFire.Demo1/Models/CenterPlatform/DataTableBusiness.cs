using HangFire.Demo1.Models.CenterPlatform;
using HangFire.Demo1.Models.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace HangFire.Demo1.Models.commom
{
    public class DataTableBusiness
    {
        private static ITestDbManager _testDbManager;

        public DataTableBusiness(ITestDbManager testDbManager)
        {
            _testDbManager = testDbManager;
        }
        public static Dictionary<string, DataTable> SetBusinessDataTable<T>(T t, string TableName, string objectName, string Upstream, out string DJBH) where T : class
        {
            var sql = string.Empty;
            DataTable tb = null;
            Dictionary<string, object> ListName = null;
            DJBH = string.Empty;
            switch (objectName)
            {
                #region//库存调整单
                case "Regulation"://库存
                    {
                        Regulation retail = t as Regulation;
                        if (string.IsNullOrWhiteSpace(retail.DM2_1))
                        {
                            retail.DM2_1 = InvoicesManage.GetCKKW(retail.DM2);//仓库库位
                        }
                        if (string.IsNullOrEmpty(retail.BYZD12))
                        {
                            retail.BYZD12 = "0.0000";
                        }
                        if (string.IsNullOrEmpty(retail.QDDM))
                        {
                            retail.QDDM = InvoicesManage.GetCANGKUValue("QDDM", retail.DM2);
                        }
                        if (string.IsNullOrWhiteSpace(retail.DJBH))
                        {
                            retail.DJBH = InvoicesManage.GetNewDJBH(TableName, retail.QDDM, retail.DM2, retail.YDJH);
                        }
                        retail.BYZD1 = InvoicesManage.GetJGSD(InvoicesManage.GetCANGKUValue("JGSD", retail.DM2)).ToString();
                        retail.DM1 = "000";
                        retail.DJXZ = "0";
                        retail.YGDM = InvoicesManage.GetCANGKUValue("YGDM", retail.DM2);
                        retail.BYZD5 = retail.BYZD1;
                        retail.JZ = "0";
                        retail.RQ_4 = DateTime.Now;
                        retail.SHRQ = retail.RQ.ToString();
                        retail.YSRQ = retail.RQ.ToString();
                        retail.YGDM = InvoicesManage.GetCANGKUValue("YGDM", retail.DM2);
                        DJBH = retail.DJBH;
                        ListName = RequestBuilder.getProperties<Regulation>(retail, objectName);
                        break;
                    }
                    #endregion
            }
            tb = InvoicesManage.SetDataTable(ListName, TableName);
            if ("SG_Gathering".Equals(objectName))
                tb.PrimaryKey = new DataColumn[] { tb.Columns["vMBillID"] };
            else
                tb.PrimaryKey = new DataColumn[] { tb.Columns["DJBH"] };
            sql = InvoicesManage.SetSQLValue(ListName, TableName, "", objectName);
            Dictionary<string, DataTable> dic = new Dictionary<string, DataTable>();
            dic.Add(sql, tb);
            return dic;
        }

        //public static Dictionary<string, DataTable> SetEntryOrderDetail_QT_2(string DJBH, string TableName, DataRow dataRow, string KHDM)
        //{
        //    int num = 0;
        //    Dictionary<string, DataTable> dictionary = new Dictionary<string, DataTable>();
        //    List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
        //    string text = string.Empty;
        //    Dictionary<string, DataTable> result;
        //    try
        //    {
        //        string sql = string.Format(@"SELECT TOP 1 TMDZB.SPDM,TMDZB.GG1DM,TMDZB.GG2DM FROM dbo.TMDZB WHERE SPDM='{0}' AND SPTM='{1}' ", dataRow["item_id"], dataRow["sku_id"]);
        //        DataTable dataTable = _testDbManager.FillData(sql);
                
        //        string sPDM = string.Empty;
        //        string gG1DM = string.Empty;
        //        string gG2DM = string.Empty;
        //        sPDM = dataTable.Rows[0]["SPDM"].ToString();
        //        gG1DM = ((dataTable.Rows[0]["GG1DM"].ToString() == "") ? _testDbManager.ExecuteScalar("select top 1 GGDM from guige1").ToString() : dataTable.Rows[0]["GG1DM"].ToString());
        //        gG2DM = ((dataTable.Rows[0]["GG2DM"].ToString() == "") ? _testDbManager.ExecuteScalar("select top 1 GGDM from guige2").ToString() : dataTable.Rows[0]["GG2DM"].ToString());
        //        PurchaseDetail purchaseDetail = new PurchaseDetail();
        //        purchaseDetail.DJBH = DJBH;
        //        purchaseDetail.SPDM = sPDM;
        //        purchaseDetail.GG1DM = gG1DM;
        //        purchaseDetail.GG2DM = gG2DM;
        //        purchaseDetail.SL = dataRow["current_amount"].ToString();
        //        purchaseDetail.SL_2 = dataRow["current_amount"].ToString();
        //        purchaseDetail.DJ = (Convert.ToDouble(dataRow["item_price"]) / 100).ToString();
        //        purchaseDetail.CKJ = (Convert.ToDouble(dataRow["item_price"]) / 100).ToString();
        //        purchaseDetail.ZK = "1.00";
        //        purchaseDetail.JE = (Convert.ToDouble(dataRow["current_amount"].ToString()) * Convert.ToDouble(dataRow["item_price"])).ToString();
        //        purchaseDetail.byzd8 = InvoicesManage.GetKEHUValue("BYZD8", KHDM);
        //        if (string.IsNullOrEmpty(purchaseDetail.byzd8))
        //        {
        //            purchaseDetail.byzd8 = "0.00";
        //        }
        //        purchaseDetail.byzd9 = (Convert.ToDouble(purchaseDetail.JE) * (1.0 - Convert.ToDouble(purchaseDetail.byzd8))).ToString();
        //        purchaseDetail.BYZD12 = purchaseDetail.CKJ;
        //        num++;
        //        purchaseDetail.MIBH = num;
        //        purchaseDetail.MXBH = num;
        //        purchaseDetail.HH = "1";
        //        purchaseDetail.byzd1 = "0";
        //        purchaseDetail.DJ_1 = purchaseDetail.CKJ;
        //        Dictionary<string, object> item = DataTableBusiness.SetInvoicesBusiness<PurchaseDetail>(purchaseDetail, TableName, "PurchaseDetail", "", "");
        //        if (string.IsNullOrEmpty(text))
        //        {
        //            text = DataTableBusiness.SetInvoicesBusiness<PurchaseDetail>(purchaseDetail, TableName + "MX", "PurchaseDetail", DJBH);
        //        }
        //        list.Add(item);
        //        DataTable value = InvoicesManage.SetDataTable(list, TableName + "MX");
        //        dictionary.Add(text, value);
        //        result = dictionary;
        //    }
        //    catch (Exception ex)
        //    {
                 
        //    }
        //    return result;
        //}
    }
}
