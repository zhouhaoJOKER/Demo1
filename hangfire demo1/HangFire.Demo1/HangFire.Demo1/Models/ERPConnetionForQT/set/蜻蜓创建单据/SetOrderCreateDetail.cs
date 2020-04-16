using BSERP.Connectors.Efast.Interface;
using BSERP.Connectors.Efast.Log;
using BSERP.Connectors.Efast.Services;
using BSERP.Connectors.Efast.Util;
using ERPApiService.Common.Business;
using ERPApiService.Common.Object;
using HangFire.Demo1.Models.ERPConnetionForQT.entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;

namespace ERPApiService.Business.ERPConnetionForQT.set
{
    /// <summary>
    /// 蜻蜓平台 消息通知-创建单据  taobao_ifashion_OrderCreate 
    /// 生成到中台 零售类小票  采购类->商品进货通知单和商品退货单
    /// </summary>
    public class SetOrderCreateDetail : StatusTask<QT_SPKCB>, IBillType
    {
		public object synObject = new object();

		public SetOrderCreateDetail()
		{
			base.Parameter["method"] = "taobao.retail.ifashion.order.get";
		}

		public override void Run()
		{
			string sql = @"SELECT top 100 storeid,orderId,orderType FROM OrderCreate WHERE ISNULL(ismove,'0')<>'1' 
			                         GROUP BY storeid,orderId,orderType,lastchanged  ORDER BY lastchanged desc";

			//string sql = @"SELECT top 100 storeid,orderId,orderType FROM OrderCreate WHERE  orderid='1142001'  
			//                         GROUP BY storeid,orderId,orderType,lastchanged  ORDER BY lastchanged desc";
			DataTable dataTable = BusinessDbUtil.GetDataTable(sql);
			if (dataTable != null && dataTable.Rows.Count > 0)
			{
				int count = dataTable.Rows.Count;
				int num = (count + 100 - 1) / 100;
				int num2 = 100;
				int num3 = 0;
				Thread[] array = new Thread[num];
				for (int i = 0; i < count; i += 100)
				{
					DataRow[] drs = dataTable.AsEnumerable().Take(num2).Skip(i).ToArray<DataRow>();
					array[num3] = new Thread(()=>
					{
						this.getOrderDetail(drs);
					});
					array[num3].Name = "线程" + i.ToString();
					array[num3].Start();
					num3++;
					num2 += 100;
				}
			}
		}
		public void getOrderDetail(DataRow[] drs)
		{
			string app_key = ConfigUtil.App_key;
			string app_secret = ConfigUtil.App_secret;
			string iposApiUrl = ConfigUtil.IposApiUrl;
			ITopClient topClient = new DefaultTopClient(iposApiUrl, app_key, app_secret);
			RetailIfashionOrderGetRequest retailIfashionOrderGetRequest = new RetailIfashionOrderGetRequest();
			for (int i =0; i < drs.Length; i++)
			{
				DataRow dataRow = drs[i];
				retailIfashionOrderGetRequest.OrderId = dataRow["orderId"].ToString();
				retailIfashionOrderGetRequest.Source = "baison";
				retailIfashionOrderGetRequest.Type = dataRow["orderType"].ToString();
				try
				{
                    #region
                        ////零售销货单
                        //if (retailIfashionOrderGetRequest.Type == "qt-sale")
                        //{
                        //	string accessToken_QT = InvoicesManage.GetAccessToken_QT(dataRow["storeid"].ToString());
                        //	RetailIfashionOrderGetResponse rsp = topClient.Execute<RetailIfashionOrderGetResponse>(retailIfashionOrderGetRequest, accessToken_QT);
                        //	if (rsp != null && rsp.Result != null && rsp.Result.Success)
                        //	{
                        //		string orderid = rsp.Result.Data.OrderId.ToString();
                        //		string KHDM = "";
                        //		string sql4 = "select top 1 KHDM,QDDM,CKDM,ZK,JGSD from kehu INNER JOIN OrderCreate ON kehu.KHDM=OrderCreate.storeId WHERE OrderCreate.orderId='" + orderid + "'";
                        //		DataTable dataTable = BusinessDbUtil.GetDataTable(sql4);
                        //		KHDM = dataTable.Rows[0]["KHDM"].ToString();
                        //		string CKDM = dataTable.Rows[0]["CKDM"].ToString();
                        //		string text = dataTable.Rows[0]["JGSD"].ToString();
                        //		string QDDM = dataTable.Rows[0]["QDDM"].ToString();
                        //		string SL = rsp.Result.Data.SkuInfoList.Sum((RetailIfashionOrderGetResponse.OrderInfoDetailDomain s) => Convert.ToDecimal(s.Amount)).ToString();
                        //		string JE = rsp.Result.Data.SkuInfoList.Sum((RetailIfashionOrderGetResponse.OrderInfoDetailDomain s) => Convert.ToDecimal(s.Amount) * Convert.ToDecimal(s.ItemPrice)).ToString();
                        //		string arg = "SELECT djbh,qddm,dm1,dm2,dm2_1,rq,sl,je,bz,zdr,rq_4,shr,sh,shrq,ygdm,byzd1,ll,byzd12,je_1,isonline FROM LSXHD";
                        //		DataTable table = BusinessDbUtil.GetDataTable(string.Format("{0} WHERE  djbh='{1}'", arg, orderid));
                        //		table.TableName = "LSXHD";
                        //		table.PrimaryKey = new DataColumn[]
                        //		{
                        //			table.Columns["djbh"]
                        //		};
                        //		string arg2 = string.Join(",", (from item1 in rsp.Result.Data.SkuInfoList
                        //										select string.Format("'{0}'", item1.SkuId)).ToArray<string>());
                        //		string sql = string.Format(@"select distinct TMDZB.SPDM,GG1DM,GG2DM,BZSJ from TMDZB inner join SHANGPIN on TMDZB.SPDM=SHANGPIN.SPDM  where TMDZB.SPTM in ({0}) ", arg2);
                        //		LogUtil.WriteInfo(this, "查询商品档案", " sql : " + sql);
                        //		DataTable dt_SP = InvoicesManage.ExecuteQuery(sql);
                        //		BusinessDbUtil.DoAction(delegate (DirectDbManger dbManager)
                        //		{
                        //			try
                        //			{
                        //				if (table.Rows.Count < 1)
                        //				{
                        //					if (ValidDJUtil.IsExistKHDM(this, orderid, KHDM))
                        //					{
                        //						string columns = SqlUtil.GetColumns(table);
                        //						DataRow dataRow2 = table.NewRow();
                        //						dataRow2["djbh"] = orderid;
                        //						dataRow2["qddm"] = QDDM;
                        //						dataRow2["dm1"] = KHDM;
                        //						dataRow2["dm2"] = CKDM;
                        //						dataRow2["dm2_1"] = "000";
                        //						dataRow2["rq"] = DateTime.Now.ToShortDateString();
                        //						dataRow2["bz"] = "";
                        //						dataRow2["sl"] = SL;
                        //						dataRow2["je"] = JE;
                        //						dataRow2["zdr"] = "QT";
                        //						dataRow2["rq_4"] = DateTime.Now.ToShortDateString();
                        //						dataRow2["ygdm"] = "000";
                        //						dataRow2["byzd1"] = ValidDJUtil.GetJGXD(this, KHDM, "JGSD");
                        //						dataRow2["ll"] = "1";
                        //						dataRow2["byzd12"] = "1";
                        //						dataRow2["isonline"] = "1";
                        //						dataRow2["je_1"] = JE;
                        //						table.Rows.Add(dataRow2);
                        //						string text6 = SqlUtil.ConvertInsert(table, columns, dataRow2);
                        //						DataTable dataTable2 = BusinessDbUtil.GetDataTable(string.Format("SELECT djbh,mibh,spdm,gg1dm,gg2dm,ckj,dj,sl,zk,bzje,je FROM LSXHDMX WHERE 1=0", new object[0]));
                        //						dataTable2.TableName = "LSXHDMX";
                        //						dataTable2.PrimaryKey = new DataColumn[]
                        //						{
                        //							dataTable2.Columns["djbh"],
                        //							dataTable2.Columns["mibh"]
                        //						};
                        //						bool flag4 = false;
                        //						for (int j = 0; j < rsp.Result.Data.SkuInfoList.Count; j++)
                        //						{
                        //							RetailIfashionOrderGetResponse.OrderInfoDetailDomain orderline = rsp.Result.Data.SkuInfoList[j];
                        //							DataRow dataRow3 = (from itemObj in dt_SP.AsEnumerable()
                        //												where itemObj["SKUDM"].ToString() == orderline.SkuId.ToString()
                        //												select itemObj).ToArray<DataRow>()[0];
                        //							string columns2 = SqlUtil.GetColumns(dataTable2);
                        //							DataRow dataRow4 = dataTable2.NewRow();
                        //							dataRow4["djbh"] = orderid;
                        //							dataRow4["mibh"] = j + 1;
                        //							dataRow4["spdm"] = orderline.ItemId;
                        //							dataRow4["gg1dm"] = dataRow3["GG1DM"].ToString();
                        //							dataRow4["gg2dm"] = dataRow3["GG2DM"].ToString();
                        //							dataRow4["dj"] = Convert.ToDecimal(Convert.ToDouble(orderline.ItemPrice)/100) / 100m;
                        //							dataRow4["ckj"] = Convert.ToDecimal(Convert.ToDouble(orderline.ItemPrice)/100) / 100m;
                        //							dataRow4["sl"] = orderline.Amount;
                        //							dataRow4["zk"] = "1";
                        //							dataRow4["je"] = Convert.ToDecimal(Convert.ToDouble(orderline.ItemPrice)/100) * Convert.ToDecimal(orderline.Amount) / 100m;
                        //							dataRow4["bzje"] = Convert.ToDecimal(Convert.ToDouble(orderline.ItemPrice)/100) * Convert.ToDecimal(orderline.Amount) / 100m;
                        //							dataTable2.Rows.Add(dataRow4);
                        //						}
                        //						if (flag4)
                        //						{
                        //							DataTable dataTable3 = BusinessDbUtil.GetDataTable(string.Format("SELECT DJBH,mibh,JSFS,JE FROM LSXHDJS WHERE 1=0", new object[0]));
                        //							dataTable3.TableName = "LSXHDJS";
                        //							dataTable3.PrimaryKey = new DataColumn[]
                        //							{
                        //								dataTable3.Columns["DJBH"],
                        //								dataTable3.Columns["mibh"]
                        //							};
                        //							DataRow dataRow5 = dataTable3.NewRow();
                        //							dataRow5["DJBH"] = orderid;
                        //							dataRow5["mibh"] = 1;
                        //							dataRow5["JSFS"] = "999";
                        //							dataRow5["JE"] = Convert.ToDecimal(JE) / 100m;
                        //							dataTable3.Rows.Add(dataRow5);
                        //							IEnumerable<string> enumerable = SqlUtil.Convert(dataTable2);
                        //							IEnumerable<string> enumerable2 = SqlUtil.Convert(dataTable3);
                        //							bool flag5 = false;
                        //							sql = dbManager.ExecuteBatchQuery(out flag5, text6, enumerable, enumerable2);
                        //							InvoicesManage.UpdateBillPrice(orderid, "LSXHD");
                        //							DataTable procedureParameter = dbManager.GetProcedureParameter("P_API_Oper_LSXHD_SH");
                        //							dbManager.SetProcedureParameter(procedureParameter, "DJBH", orderid);
                        //							dbManager.SetProcedureParameter(procedureParameter, "USER", "QT");
                        //							dbManager.SetProcedureParameter(procedureParameter, "iRet", "1");
                        //							dbManager.SetProcedureParameter(procedureParameter, "rq", DateTime.Now.ToShortDateString());
                        //							DataTable dataTable4 = dbManager.ExecProcedure("P_API_Oper_LSXHD_SH", procedureParameter);
                        //							if (flag5 && dataTable4 != null && dataTable4.Rows.Count > 0)
                        //							{
                        //								LogUtil.WriteInfo(this, "", "单据保存成功,对应单据编号为:" + orderid);
                        //							}
                        //							else
                        //							{
                        //								LogUtil.WriteInfo(this, "", string.Concat(new object[]
                        //								{
                        //									"单据保存失败,对应sql语句为:",
                        //									text6,
                        //									enumerable,
                        //									enumerable2
                        //								}));
                        //							}
                        //						}
                        //					}
                        //				}
                        //				else
                        //				{
                        //					LogUtil.WriteInfo(this, "", "单据已存在,对应单据编号为:" + orderid);
                        //				}
                        //			}
                        //			catch (Exception ex2)
                        //			{
                        //				LogUtil.WriteError(this, sql, ex2, "");
                        //			}
                        //		});
                        //		string sql2 = "update OrderCreate set is_move='1' where storeid='" + dataRow["storeid"].ToString() + "' and  orderId ='" + dataRow["storeid"].ToString() + "'";
                        //		BusinessDbUtil.ExecuteNonQuery(sql2);
                        //	}
                        //	else
                        //	{
                        //		LogUtil.WriteInfo(this, rsp.Body, string.Format("返回的状态：flag:{0},code:{1},message:{2}", rsp.Body, rsp.Body, rsp.Body));
                        //	}
                        //}
                        ////零售退货单
                        //else if (retailIfashionOrderGetRequest.Type == "qt-sale-back")
                        //{
                        //	string accessToken_QT = InvoicesManage.GetAccessToken_QT(dataRow["storeid"].ToString());
                        //	RetailIfashionOrderGetResponse rsp = topClient.Execute<RetailIfashionOrderGetResponse>(retailIfashionOrderGetRequest, accessToken_QT);
                        //	if (rsp != null && rsp.Result != null && rsp.Result.Success)
                        //	{
                        //		string orderid = rsp.Result.Data.OrderId.ToString();
                        //		string KHDM = "";
                        //		string sql4 = "select top 1 KHDM,QDDM,CKDM,ZK,JGSD from kehu INNER JOIN OrderCreate ON kehu.KHDM=OrderCreate.storeId WHERE OrderCreate.orderId='" + orderid + "'";
                        //		DataTable dataTable = BusinessDbUtil.GetDataTable(sql4);
                        //		KHDM = dataTable.Rows[0]["KHDM"].ToString();
                        //		string CKDM = dataTable.Rows[0]["CKDM"].ToString();
                        //		string text = dataTable.Rows[0]["JGSD"].ToString();
                        //		string QDDM = dataTable.Rows[0]["QDDM"].ToString();
                        //		string SL = rsp.Result.Data.SkuInfoList.Sum((RetailIfashionOrderGetResponse.OrderInfoDetailDomain s) => Convert.ToDecimal(s.Amount)).ToString();
                        //		string JE = rsp.Result.Data.SkuInfoList.Sum((RetailIfashionOrderGetResponse.OrderInfoDetailDomain s) => Convert.ToDecimal(s.Amount) * Convert.ToDecimal(s.ItemPrice)).ToString();
                        //		string arg = "SELECT djbh,qddm,dm1,dm2,dm2_1,rq,sl,je,bz,zdr,rq_4,shr,sh,shrq,ygdm,byzd1,ll,byzd12,je_1,isonline FROM LSTHD";
                        //		DataTable table = BusinessDbUtil.GetDataTable(string.Format("{0} WHERE  djbh='{1}'", arg, orderid));
                        //		table.TableName = "LSXHD";
                        //		table.PrimaryKey = new DataColumn[]
                        //		{
                        //			table.Columns["djbh"]
                        //		};
                        //		string arg2 = string.Join(",", (from item1 in rsp.Result.Data.SkuInfoList
                        //										select string.Format("'{0}'", item1.SkuId)).ToArray<string>());
                        //		string sql = string.Format(@"select distinct TMDZB.SPDM,GG1DM,GG2DM,BZSJ from TMDZB inner join SHANGPIN on TMDZB.SPDM=SHANGPIN.SPDM  where TMDZB.SPTM in ({0}) ", arg2);
                        //		LogUtil.WriteInfo(this, "查询商品档案", " sql : " + sql);
                        //		DataTable dt_SP = InvoicesManage.ExecuteQuery(sql);
                        //		BusinessDbUtil.DoAction(delegate (DirectDbManger dbManager)
                        //		{
                        //			try
                        //			{
                        //				if (table.Rows.Count < 1)
                        //				{
                        //					if (ValidDJUtil.IsExistKHDM(this, orderid, KHDM))
                        //					{
                        //						string columns = SqlUtil.GetColumns(table);
                        //						DataRow dataRow2 = table.NewRow();
                        //						dataRow2["djbh"] = orderid;
                        //						dataRow2["qddm"] = QDDM;
                        //						dataRow2["dm1"] = KHDM;
                        //						dataRow2["dm2"] = CKDM;
                        //						dataRow2["dm2_1"] = "000";
                        //						dataRow2["rq"] = DateTime.Now.ToShortDateString();
                        //						dataRow2["bz"] = "";
                        //						dataRow2["sl"] = SL;
                        //						dataRow2["je"] = JE;
                        //						dataRow2["zdr"] = "QT";
                        //						dataRow2["rq_4"] = DateTime.Now.ToShortDateString();
                        //						dataRow2["ygdm"] = "000";
                        //						dataRow2["byzd1"] = ValidDJUtil.GetJGXD(this, KHDM, "JGSD");
                        //						dataRow2["ll"] = "1";
                        //						dataRow2["byzd12"] = "1";
                        //						dataRow2["isonline"] = "1";
                        //						dataRow2["je_1"] = JE;
                        //						table.Rows.Add(dataRow2);
                        //						string text6 = SqlUtil.ConvertInsert(table, columns, dataRow2);
                        //						DataTable dataTable2 = BusinessDbUtil.GetDataTable(string.Format("SELECT djbh,mibh,spdm,gg1dm,gg2dm,ckj,dj,sl,zk,bzje,je FROM LSTHDMX WHERE 1=0", new object[0]));
                        //						dataTable2.TableName = "LSTHDMX";
                        //						dataTable2.PrimaryKey = new DataColumn[]
                        //						{
                        //							dataTable2.Columns["djbh"],
                        //							dataTable2.Columns["mibh"]
                        //						};
                        //						bool flag4 = false;
                        //						for (int j = 0; j < rsp.Result.Data.SkuInfoList.Count; j++)
                        //						{
                        //							RetailIfashionOrderGetResponse.OrderInfoDetailDomain orderline = rsp.Result.Data.SkuInfoList[j];
                        //							DataRow dataRow3 = (from itemObj in dt_SP.AsEnumerable()
                        //												where itemObj["SKUDM"].ToString() == orderline.SkuId.ToString()
                        //												select itemObj).ToArray<DataRow>()[0];
                        //							string columns2 = SqlUtil.GetColumns(dataTable2);
                        //							DataRow dataRow4 = dataTable2.NewRow();
                        //							dataRow4["djbh"] = orderid;
                        //							dataRow4["mibh"] = j + 1;
                        //							dataRow4["spdm"] = orderline.ItemId;
                        //							dataRow4["gg1dm"] = dataRow3["GG1DM"].ToString();
                        //							dataRow4["gg2dm"] = dataRow3["GG2DM"].ToString();
                        //							dataRow4["dj"] = Convert.ToDecimal(Convert.ToDouble(orderline.ItemPrice)/100) / 100m;
                        //							dataRow4["ckj"] = Convert.ToDecimal(Convert.ToDouble(orderline.ItemPrice)/100) / 100m;
                        //							dataRow4["sl"] = orderline.Amount;
                        //							dataRow4["zk"] = "1";
                        //							dataRow4["je"] = Convert.ToDecimal(Convert.ToDouble(orderline.ItemPrice)/100) * Convert.ToDecimal(orderline.Amount) / 100m;
                        //							dataRow4["bzje"] = Convert.ToDecimal(Convert.ToDouble(orderline.ItemPrice)/100) * Convert.ToDecimal(orderline.Amount) / 100m;
                        //							dataTable2.Rows.Add(dataRow4);
                        //						}
                        //						if (flag4)
                        //						{
                        //							DataTable dataTable3 = BusinessDbUtil.GetDataTable(string.Format("SELECT DJBH,mibh,JSFS,JE FROM LSTHDJS WHERE 1=0", new object[0]));
                        //							dataTable3.TableName = "LSTHDJS";
                        //							dataTable3.PrimaryKey = new DataColumn[]
                        //							{
                        //								dataTable3.Columns["DJBH"],
                        //								dataTable3.Columns["mibh"]
                        //							};
                        //							DataRow dataRow5 = dataTable3.NewRow();
                        //							dataRow5["DJBH"] = orderid;
                        //							dataRow5["mibh"] = 1;
                        //							dataRow5["JSFS"] = "999";
                        //							dataRow5["JE"] = Convert.ToDecimal(JE) / 100m;
                        //							dataTable3.Rows.Add(dataRow5);
                        //							IEnumerable<string> enumerable = SqlUtil.Convert(dataTable2);
                        //							IEnumerable<string> enumerable2 = SqlUtil.Convert(dataTable3);
                        //							bool flag5 = false;
                        //							sql = dbManager.ExecuteBatchQuery(out flag5, text6, enumerable, enumerable2);
                        //							InvoicesManage.UpdateBillPrice(orderid, "LSTHD");
                        //							DataTable procedureParameter = dbManager.GetProcedureParameter("P_API_Oper_LSTHD_SH");
                        //							dbManager.SetProcedureParameter(procedureParameter, "DJBH", orderid);
                        //							dbManager.SetProcedureParameter(procedureParameter, "USER", "QT");
                        //							dbManager.SetProcedureParameter(procedureParameter, "iRet", "1");
                        //							dbManager.SetProcedureParameter(procedureParameter, "rq", DateTime.Now.ToShortDateString());
                        //							DataTable dataTable4 = dbManager.ExecProcedure("P_API_Oper_LSTHD_SH", procedureParameter);
                        //							if (flag5 && dataTable4 != null && dataTable4.Rows.Count > 0)
                        //							{
                        //								LogUtil.WriteInfo(this, "", "单据保存成功,对应单据编号为:" + orderid);
                        //							}
                        //							else
                        //							{
                        //								LogUtil.WriteInfo(this, "", string.Concat(new object[]
                        //								{
                        //									"单据保存失败,对应sql语句为:",
                        //									text6,
                        //									enumerable,
                        //									enumerable2
                        //								}));
                        //							}
                        //						}
                        //					}
                        //				}
                        //				else
                        //				{
                        //					LogUtil.WriteInfo(this, "", "单据已存在,对应单据编号为:" + orderid);
                        //				}
                        //			}
                        //			catch (Exception ex2)
                        //			{
                        //				LogUtil.WriteError(this, sql, ex2, "");
                        //			}
                        //		});
                        //		string sql2 = "update OrderCreate set is_move='1' where storeid='" + dataRow["storeid"].ToString() + "' and  orderId ='" + dataRow["storeid"].ToString() + "'";
                        //		BusinessDbUtil.ExecuteNonQuery(sql2);
                        //	}
                        //	else
                        //	{
                        //		LogUtil.WriteInfo(this, rsp.Body, string.Format("返回的状态：flag:{0},code:{1},message:{2}", rsp.Body, rsp.Body, rsp.Body));
                        //	}
                        //}
                        ////商品进货通知单
                        //else if (retailIfashionOrderGetRequest.Type == "qt-stock-in")
                        //{
                        //	string accessToken_QT = InvoicesManage.GetAccessToken_QT(dataRow["storeid"].ToString());
                        //	string text2 = "JSEND";
                        //	RetailIfashionOrderGetResponse retailIfashionOrderGetResponse = topClient.Execute<RetailIfashionOrderGetResponse>(retailIfashionOrderGetRequest, accessToken_QT);
                        //	if (retailIfashionOrderGetResponse != null && retailIfashionOrderGetResponse.Result != null && retailIfashionOrderGetResponse.Result.Success)
                        //	{
                        //		string empty = string.Empty;
                        //		Dictionary<string, DataTable> dictionary = new Dictionary<string, DataTable>();
                        //		Dictionary<string, DataTable> dictionary2 = new Dictionary<string, DataTable>();
                        //		List<Dictionary<string, DataTable>> list = new List<Dictionary<string, DataTable>>();
                        //		List<YanShouInfo> list2 = new List<YanShouInfo>();
                        //		string text3 = retailIfashionOrderGetResponse.Result.Data.OrderId.ToString();
                        //		string sql4 = "select top 1 KHDM,QDDM,CKDM,ZK,JGSD from kehu INNER JOIN OrderCreate ON kehu.KHDM=OrderCreate.storeId WHERE OrderCreate.orderId='" + text3 + "'";
                        //		DataTable dataTable = BusinessDbUtil.GetDataTable(sql4);
                        //		string zPSD = dataTable.Rows[0]["KHDM"].ToString();
                        //		string dM = dataTable.Rows[0]["CKDM"].ToString();
                        //		string text = dataTable.Rows[0]["JGSD"].ToString();
                        //		string qDDM = dataTable.Rows[0]["QDDM"].ToString();
                        //		Purchase shopinfo = new Purchase();
                        //		shopinfo.QDDM = qDDM;
                        //		string sql3 = "SELECT TOP 1 GHSDM FROM gonghuoshang WHERE ISNULL(TZSY,'0')<>'1' ORDER BY GHSDM desc";
                        //		string dM2 = BusinessDbUtil.ExecuteScalar(sql3).ToString();
                        //		shopinfo.DM1 = dM2;
                        //		shopinfo.DM2 = dM;
                        //		shopinfo.DM2_1 = "000";
                        //		shopinfo.LXDJ = "";
                        //		shopinfo.DM4 = "ZP";
                        //		shopinfo.QYDM = "000";
                        //		shopinfo.BYZD1 = "0";
                        //		shopinfo.FPLX = "3";
                        //		shopinfo.YGDM = "000";
                        //		shopinfo.BYZD3 = "";
                        //		shopinfo.BYZD12 = "1";
                        //		shopinfo.YGDM = "000";
                        //		shopinfo.isonline = "1";
                        //		shopinfo.DJBH = InvoicesManage.GetNewDJBH(text2, shopinfo.QDDM, shopinfo.DM1, text3);
                        //		shopinfo.ZPSD = zPSD;
                        //		string text4 = shopinfo.DJBH;
                        //		shopinfo.SHR = "QT";
                        //		shopinfo.ZDR = "QT";
                        //		shopinfo.RQ = DateTime.Now.ToString("yyyy-MM-dd");
                        //		//原单据 
                        //		shopinfo.YDJH = text3;
                        //		//先不验收 
                        //		//list2.Add(new YanShouInfo
                        //		//{
                        //		//	DJBH = shopinfo.DJBH,
                        //		//	TableName = text2,
                        //		//	User = "QT",
                        //		//	Procedure = string.Format("P_API_Oper_{0}_YS", text2),
                        //		//	BYZD3 = shopinfo.BYZD3
                        //		//});
                        //		//list2.Add(new YanShouInfo
                        //		//{
                        //		//	DJBH = shopinfo.DJBH,
                        //		//	TableName = text2,
                        //		//	User = "QT",
                        //		//	Procedure = string.Format("P_API_Oper_{0}_JZ", text2),
                        //		//	BYZD3 = shopinfo.BYZD3
                        //		//});
                        //		dictionary = DataTableBusiness.SetBusinessDataTable<Purchase>(shopinfo, text2, "Purchase", text2);
                        //		List<RetailIfashionOrderGetResponse.OrderInfoDetailDomain> skuInfoList = retailIfashionOrderGetResponse.Result.Data.SkuInfoList;
                        //		dictionary2 = DataTableBusiness.SetEntryOrderDetail_QT(shopinfo.DJBH, text2, skuInfoList, shopinfo.DM1);
                        //		if (dictionary.Count > 0 && dictionary2.Count > 0)
                        //		{
                        //			list.Add(dictionary);
                        //			list.Add(dictionary2);
                        //		}
                        //		if (list.Count > 0)
                        //		{
                        //			try
                        //			{
                        //				bool flag = DataTableBusiness.SavaBusinessData_SqlParameter(list, list2);
                        //				if (flag)
                        //				{
                        //					//BusinessDbUtil.DoAction(delegate (DirectDbManger dbManager)
                        //					//{
                        //					//	DataTable procedureParameter = dbManager.GetProcedureParameter("p_JSEND_ZP");
                        //					//	dbManager.SetProcedureParameter(procedureParameter, "DJBH", shopinfo.DJBH);
                        //					//	dbManager.SetProcedureParameter(procedureParameter, "rq", DateTime.Now.ToString("yyyy-MM-dd"));
                        //					//	dbManager.SetProcedureParameter(procedureParameter, "ZDR", "QT");
                        //					//	DataTable dataTable2 = dbManager.ExecProcedure("p_JSEND_ZP", procedureParameter);
                        //					//});
                        //					LogUtil.WriteInfo(this, "0", string.Format("ERP业务单据{0}创建成功!对应的仓储系统的出库单号:{1}保存成功", text4, text3));

                        //					string sql2 = "update OrderCreate set is_move='1' where storeid='" + dataRow["storeid"].ToString() + "' and  orderId ='" + dataRow["storeid"].ToString() + "'";
                        //					BusinessDbUtil.ExecuteNonQuery(sql2);
                        //				}
                        //				else
                        //				{
                        //					LogUtil.WriteError(this, "failure", string.Format("保存单据失败，请检查系统日志！ {0}", text3));
                        //				}
                        //			}
                        //			catch (Exception ex)
                        //			{
                        //				LogUtil.WriteError(this, "failure", string.Format("保存单据失败，请检查系统日志！ {0} + BusinessList : {1}", text3, JsonParser.ToJson(list)));
                        //			}
                        //		}
                        //		else
                        //		{
                        //			LogUtil.WriteError(this, "failure", string.Format("保存单据失败,无保存数据,请检查系统日志！", new object[0]));
                        //		}
                        //	}
                        //}
                        #endregion
                   //商品退货通知单
                    if (retailIfashionOrderGetRequest.Type == "qt-stock-back")
					{
						string accessToken_QT = InvoicesManage.GetAccessToken_QT(dataRow["storeid"].ToString());
						RetailIfashionOrderGetResponse retailIfashionOrderGetResponse = topClient.Execute<RetailIfashionOrderGetResponse>(retailIfashionOrderGetRequest, accessToken_QT);
						if (retailIfashionOrderGetResponse != null && retailIfashionOrderGetResponse.Result != null && retailIfashionOrderGetResponse.Result.Success)
						{
							string text2 = "JTSND";
							lock (this.synObject)
							{
								string empty = string.Empty;
								Dictionary<string, DataTable> dictionary = new Dictionary<string, DataTable>();
								Dictionary<string, DataTable> dictionary2 = new Dictionary<string, DataTable>();
								List<Dictionary<string, DataTable>> list = new List<Dictionary<string, DataTable>>();
								List<YanShouInfo> list3 = new List<YanShouInfo>();
								List<YanShouInfo> list2 = new List<YanShouInfo>();
								string text3 = retailIfashionOrderGetResponse.Result.Data.OrderId.ToString();
								if (!InvoicesManage.IsbillsExist("JTSND", text3))
								{
									Purchase purchase = new Purchase();
									string sql4 = "select top 1 KHDM,QDDM,CKDM,ZK,JGSD from kehu INNER JOIN OrderCreate ON kehu.KHDM= cast(OrderCreate.storeId as varchar(50)) WHERE cast(OrderCreate.orderId as varchar(50))='" + text3 + "'";
									DataTable dataTable = BusinessDbUtil.GetDataTable(sql4);
									string zPSD = dataTable.Rows[0]["KHDM"].ToString();
									string dM = dataTable.Rows[0]["CKDM"].ToString();
									string text = dataTable.Rows[0]["JGSD"].ToString();
									string qDDM = dataTable.Rows[0]["QDDM"].ToString();
									purchase.QDDM = qDDM;

									purchase.LXDJ = "";
									purchase.DM4 = "002";
									purchase.DM5 = "";
									purchase.FZXX = "";
									purchase.DM2_1 = "000";
									//添加虚拟总仓
									purchase.DM2 = "QT_XNZC";
									purchase.BYZD1 = "0";
									purchase.FPLX = "3";
									purchase.YGDM = "000";
									string text5 = "QT";
									purchase.DJBH = InvoicesManage.GetNewDJBH("JTSND", purchase.QDDM, purchase.DM1, text3);
									purchase.SHR = text5;
									purchase.RQ = DateTime.Now.ToString("yyyy-MM-dd");
									purchase.YDJH = text3;
									purchase.ZDR = "QT";
									purchase.SL = retailIfashionOrderGetResponse.Result.Data.SkuInfoList.Sum((RetailIfashionOrderGetResponse.OrderInfoDetailDomain item) => (int)Convert.ToInt16(item.Amount)).ToString();
									purchase.JE = retailIfashionOrderGetResponse.Result.Data.TotalFee.ToString();
									List<RetailIfashionOrderGetResponse.OrderInfoDetailDomain> skuInfoList = retailIfashionOrderGetResponse.Result.Data.SkuInfoList;
									//获取供货商代码
									string sql3 = string.Format(@"select top 1 GHSDM from GONGHUOSHANG where GHSMC in (select top  1 shop_name from storeskulist
                                                  where store_id='{0}' and item_id='{1}' and sku_id='{2}')", zPSD, skuInfoList[0].ItemId, skuInfoList[0].SkuId);

									string dM2 = BusinessDbUtil.ExecuteScalar(sql3).ToString();
									LogUtil.WriteError(this, "false", "获取供货商代码的sql:" + sql3 + "DM2:" + dM2);
									purchase.DM1 = dM2;
									purchase.BYZD1 = InvoicesManage.GetJGSD(InvoicesManage.GetTableFiled("GONGHUOSHANG", "JGSD", " GHSDM='" + dM2 + "'")).ToString();

									dictionary = DataTableBusiness.SetBusinessDataTable<Purchase>(purchase, "JTSND", "Purchase", "SPTHD");
									dictionary2 = DataTableBusiness.SetEntryOrderDetail_QT(purchase.DJBH, text2, skuInfoList, purchase.DM1);
									string text4 = string.Empty;
									if (dictionary.Count > 0 || dictionary2.Count > 0)
									{
										list.Add(dictionary);
										list.Add(dictionary2);
									}
									if (list.Count > 0)
									{
										bool flag3 = DataTableBusiness.SavaBusinessData_SqlParameter(list, list2);

										string sql2 = "update OrderCreate set ismove='1' where cast(storeid as varchar(50))='" + dataRow["storeid"].ToString() + "' and  cast(orderId as varchar(50)) ='" + dataRow["orderId"].ToString() + "'";
										BusinessDbUtil.ExecuteNonQuery(sql2);

										sql2 = "update JTSND set JZ='1',JZR='QT',JZRQ= getdate(),YS='1',YSR='QT',YSRQ= getdate() where DJBH='" + purchase.DJBH + "'";
										BusinessDbUtil.ExecuteNonQuery(sql2);

										if (flag3)
										{
											if (list2.Count > 0)
											{
												text4 = list2[0].DJBH;
											}
											if (list3.Count > 0)
											{
												if (string.IsNullOrEmpty(text4))
												{
													text4 = list3[0].DJBH;
												}
											}
											LogUtil.WriteInfo(this, "true", string.Format("ERP业务单据{0}创建成功!对应的仓储系统的出库单号:{1}保存成功", text4, text3));
										}
										else
										{
											LogUtil.WriteError(this, "false", "保存单据失败，请检查系统日志！");
										}
									}
									else
									{
										LogUtil.WriteError(this, "false", "保存单据失败，请检查系统日志！");
									}
								}
								else
								{
									LogUtil.WriteError(this, "false", "保存单据失败，请检查系统日志！JTSND 单据已存在！"+ text3);
								}
							}
						}
						else 
						{
							LogUtil.WriteError(this, "false", "保存单据失败，请检查系统日志！"+ retailIfashionOrderGetResponse.Body);
						}
					}
					//销售单生成小票单
					else if (retailIfashionOrderGetRequest.Type == "qt-sale")
					{
						string accessToken_QT = InvoicesManage.GetAccessToken_QT(dataRow["storeid"].ToString());
						RetailIfashionOrderGetResponse rsp = topClient.Execute<RetailIfashionOrderGetResponse>(retailIfashionOrderGetRequest, accessToken_QT);
						//RetailIfashionOrderGetResponse rsp = JsonParser.FromJson<RetailIfashionOrderGetResponse>("{\"result\":{\"data\":{\"order_id\":\"906922560355647920\",\"sku_info_list\":{\"order_info_detail\":[{\"amount\":\"1\",\"item_id\":594932859276,\"item_price\":\"5700\",\"sku_id\":4287596926672}]},\"total_fee\":5700,\"type\":\"qt-sale\"},\"success\":true},\"request_id\":\"10ixfuwmszsbc\"}");
						//生成小票 查询明细 生成具体的小票
						if (rsp != null && rsp.Result != null && rsp.Result.Success)
						{
							RetailIfashionOrderGetResponse.DataDomain billData = rsp.Result.Data;
							//明细数量 
							string amount = billData.SkuInfoList.Sum(item => Convert.ToInt32(item.Amount)).ToString();
							string KHDM = "";
							string sql4 = "select top 1 KHDM,QDDM,CKDM,ZK,JGSD from kehu INNER JOIN OrderCreate ON kehu.KHDM=OrderCreate.storeId WHERE OrderCreate.orderId = '" + billData.OrderId + "'";
							DataTable dataTable = BusinessDbUtil.GetDataTable(sql4);
							KHDM = dataTable.Rows[0]["KHDM"].ToString();
							string CKDM = dataTable.Rows[0]["CKDM"].ToString();
							string text = dataTable.Rows[0]["JGSD"].ToString();
							string QDDM = dataTable.Rows[0]["QDDM"].ToString();

							text = "SELECT TOP 1 DYDM FROM dianyuan WHERE ISNULL(BYZD4,'0')<>'1' ORDER BY DYDM desc";
							string DYDM = BusinessDbUtil.ExecuteScalar(text).ToString();

							//text = "SELECT top 1  DYDM FROM UR_USERS_DIANYUAN  order BY DYDM desc";
							//string DYDM_YYY = BusinessDbUtil.ExecuteScalar(text).ToString();
						    string DYDM_YYY = KHDM + "_000"; 
							string selectsql = "SELECT vmbillid,vpfcode,vshop,vspcode,vvipcard,fquantity,fmoney,fchange,vsposition,dtdate,bPutup,bCancel,bTotal,fRealMoney,fGetMoney,fCutMoney,vEmpCode,isonline FROM SG_Gathering";
							var table = BusinessDbUtil.GetDataTable(string.Format("{0} WHERE  vMBillID='{1}'", selectsql, billData.OrderId));
							table.TableName = "SG_Gathering";
							table.PrimaryKey = new DataColumn[] { table.Columns["vmbillid"] };
							BusinessDbUtil.DoAction(dbManager =>
							{
								string sql = string.Empty;
								try
								{
									if (table.Rows.Count < 1)
									{
										//主表
										if (!ValidDJUtil.IsExistKHDM(this, billData.OrderId, dataRow["storeid"].ToString()))
										{
											//Message = JsonHelper.SuccessXmlMsg("failure", "-1", "客户代码不存在！"); 
										}

										string strColumns = SqlUtil.GetColumns(table);
										DataRow row = table.NewRow();
										row["vmbillid"] = billData.OrderId;
										row["vpfcode"] = "0";
										row["vshop"] = KHDM;
										//营业员
										row["vspcode"] = DYDM_YYY;
										row["vvipcard"] = "";
										row["fquantity"] = amount;
										row["fmoney"] = billData.TotalFee/100;
										row["fchange"] = 0;
										row["vsposition"] = "000";
										row["dtdate"] = DateTime.Now.ToShortDateString();
										row["bPutup"] = 0;  //挂单
										row["bCancel"] = 0; //作废
										row["bTotal"] = 0; //汇总
										row["fRealMoney"] = billData.TotalFee/100;
										row["fGetMoney"] = billData.TotalFee/100;
										row["fCutMoney"] = 0;
										row["vEmpCode"] = DYDM;
										row["isonline"] = "1";
										table.Rows.Add(row);
										string masterSql = SqlUtil.ConvertInsert(table, strColumns, row);

										//明细
										//明细
										var tablemx = BusinessDbUtil.GetDataTable(string.Format(
													@"SELECT vMBillID,vStyle,vColor,vSize,fPrice,fOriginPrice,fRealPrice,fQuantity,fMoney,fRealMoney,fRebate,vLBillID,vPFCode,vShop,vEmpCode FROM SG_Gatherings WHERE 1=0"));
										tablemx.TableName = "SG_Gatherings";
										tablemx.PrimaryKey = new DataColumn[] { tablemx.Columns["vMBillID"], tablemx.Columns["vLBillID"] };
										bool sku = false;
										for (int j = 0; j < billData.SkuInfoList.Count; j++)
										{
											var orderline = billData.SkuInfoList[j];
											//验证SKU是否存在
											//sku = ValidDJUtil.IsExistSku(this, orderInfo.orderCode, orderline.productCode, orderline.color, orderline.size);
											//if (!sku) break;
											var guige = BusinessDbUtil.GetDataTable(string.Format(@"select top 1 GG1DM,GG2DM from TMDZB where   SPTM='{0}' ", billData.SkuInfoList[i].SkuId));

											string fhColumns = SqlUtil.GetColumns(tablemx);
											DataRow fhrow = tablemx.NewRow();
											fhrow["vMBillID"] = billData.OrderId;
											fhrow["vStyle"] = orderline.ItemId;
											fhrow["vColor"] = guige.Rows[0]["GG1DM"];
											fhrow["vSize"] = guige.Rows[0]["GG2DM"];
											fhrow["fPrice"] = Convert.ToDouble(orderline.ItemPrice) / 100;
											fhrow["fOriginPrice"] = Convert.ToDouble(orderline.ItemPrice) / 100;
											fhrow["fRealPrice"] = Convert.ToDouble(orderline.ItemPrice)/100;
											fhrow["fQuantity"] = orderline.Amount;
											fhrow["fMoney"] = Convert.ToDouble(orderline.ItemPrice) / 100 * Convert.ToDouble(orderline.Amount);
											fhrow["fRealMoney"] = Convert.ToDouble(orderline.ItemPrice) / 100 * Convert.ToDouble(orderline.Amount);
											fhrow["fRebate"] = 1;
											fhrow["vLBillID"] = i + 1;
											fhrow["vPFCode"] = "000";
											fhrow["vShop"] = KHDM;
											fhrow["vEmpCode"] = DYDM;
											tablemx.Rows.Add(fhrow);
										}

										//if (!sku)
										//{
										//	Message = JsonHelper.SuccessXmlMsg("failure", "-1", "商品信息不存在！");
										//	return;
										//}

										//结算明细
										var jsmx = BusinessDbUtil.GetDataTable(string.Format(
												   @"SELECT vLBillID,vMBillID,vShop,vBalCode,fMoney,vPFCode,vEmpCode,fChange FROM SG_PayMethod WHERE 1=0"));
										jsmx.TableName = "SG_PayMethod";
										jsmx.PrimaryKey = new DataColumn[] { jsmx.Columns["vMBillID"], jsmx.Columns["vLBillID"] };

										DataRow fhrowjs = jsmx.NewRow();
										fhrowjs["vMBillID"] = billData.OrderId;
										fhrowjs["vLBillID"] = i + 1;
										fhrowjs["vShop"] = KHDM;
										fhrowjs["vBalCode"] = "000";
										fhrowjs["fMoney"] = billData.TotalFee / 100;
										fhrowjs["vPFCode"] = 1;
										fhrowjs["vEmpCode"] = DYDM;
										fhrowjs["fChange"] = "0";
										jsmx.Rows.Add(fhrowjs);

										IEnumerable<string> sqlList = SqlUtil.Convert(tablemx);
										IEnumerable<string> sqlList2 = SqlUtil.Convert(jsmx);

										bool flag = false;
										try
										{
											LogUtil.WriteInfo(this, "", "masterSql语句为:" + masterSql + "");
											var logsql = string.Empty;
											foreach (string sqlItem in sqlList)
											{
												logsql = logsql + sqlItem + ';';
											}
											LogUtil.WriteInfo(this, "", "logsql语句为:" + logsql + "");
											foreach (string sqlItem in sqlList2)
											{
												logsql = logsql + sqlItem + ';';
											}
											LogUtil.WriteInfo(this, "", "logsql语句为:" + logsql + "");

											sql = dbManager.ExecuteBatchQuery(out flag, masterSql, sqlList, sqlList2);
										}
										catch (Exception ex)
										{
											LogUtil.WriteError(this, "sql语句执行", ex.Message);
										}
										sql = string.Format("UPDATE SG_Gathering SET fMoney=(SELECT SUM(fMoney) FROM SG_Gatherings WHERE vMBillID='{0}') WHERE vMBillID='{0}'", billData.OrderId);
										dbManager.ExecuteNonQuery(sql);


										string sql2 = "update OrderCreate set ismove='1' where cast(storeid as varchar(50))='" + dataRow["storeid"].ToString() + "' and  cast(orderId as varchar(50)) ='" + dataRow["orderId"].ToString() + "'";
										BusinessDbUtil.ExecuteNonQuery(sql2);
										//上传小票接口
										LogUtil.WriteInfo(this, "创建小票成功 小票单号 :" + billData.OrderId, "单据保存成功,对应单据编号为:" + billData.OrderId);
										//实时更新库存
										//DataTable dtParam = dbManager.GetProcedureParameter("P_GETPOSDJKCDATA");
										//dbManager.SetProcedureParameter(dtParam, "DJBH", orderInfo.orderCode);
										//dbManager.SetProcedureParameter(dtParam, "CKDM", orderInfo.storeCode);
										//dbManager.SetProcedureParameter(dtParam, "TableName", "QTLSD");
										//dbManager.SetProcedureParameter(dtParam, "EditType", "0");

										//DataTable resultTable = dbManager.ExecProcedure("P_GETPOSDJKCDATA", dtParam);

										//if (flag && resultTable != null && resultTable.Rows.Count > 0)
										//{
										//	LogUtil.WriteInfo(this, sRequest, "单据保存成功,对应单据编号为:" + orderInfo.orderCode + "");
										//	Message = JsonHelper.SuccessXmlMsg("success", "0", "单据保存成功！");
										//}
										//else
										//{
										//	LogUtil.WriteInfo(this, sRequest, "单据保存失败,对应sql语句为:" + sql + "");
										//	Message = JsonHelper.SuccessXmlMsg("failure", "-1", "单据保存失败！");
										//}
									}
									else
									{
										//LogUtil.WriteInfo(this, sRequest, "单据已存在,对应单据编号为:" + orderInfo.orderCode + "");
										//Message = JsonHelper.SuccessXmlMsg("failure", "-1", "单据已存在！");
									}
								}
								catch (Exception ex)
								{
									LogUtil.WriteError(this, sql, ex);

									//Msg = JsonHelper.SuccessXmlMsg("failure", "-1", ex.Message);
									//result = false;
								}
							});
						}
						else
						{
							LogUtil.WriteInfo(this, rsp.Body, string.Format("返回的状态：flag:{0},code:{1},message:{2}", rsp.Body, rsp.Body, rsp.Body));
						}
					}
					//零售退货单小票单 
					else if (retailIfashionOrderGetRequest.Type == "qt-sale-back") 
					{
						string accessToken_QT = InvoicesManage.GetAccessToken_QT(dataRow["storeid"].ToString());
						RetailIfashionOrderGetResponse rsp = topClient.Execute<RetailIfashionOrderGetResponse>(retailIfashionOrderGetRequest, accessToken_QT);
						//生成小票 查询明细 生成具体的小票
						if (rsp != null && rsp.Result != null && rsp.Result.Success)
						{
							RetailIfashionOrderGetResponse.DataDomain billData = rsp.Result.Data;
							//明细数量 
							double amount = billData.SkuInfoList.Sum(item => Convert.ToDouble(item.Amount));
							string KHDM = "";
							string sql4 = "select top 1 KHDM,QDDM,CKDM,ZK,JGSD from kehu INNER JOIN OrderCreate ON kehu.KHDM=OrderCreate.storeId WHERE OrderCreate.orderId='" + billData.OrderId + "'";
							DataTable dataTable = BusinessDbUtil.GetDataTable(sql4);
							KHDM = dataTable.Rows[0]["KHDM"].ToString();
							string CKDM = dataTable.Rows[0]["CKDM"].ToString();
							string text = dataTable.Rows[0]["JGSD"].ToString();
							string QDDM = dataTable.Rows[0]["QDDM"].ToString();

							text = "SELECT TOP 1 DYDM FROM dianyuan WHERE ISNULL(BYZD4,'0')<>'1' ORDER BY DYDM desc";
							string DYDM = BusinessDbUtil.ExecuteScalar(text).ToString();

							string  DYDM_YYY = KHDM + "_000"; 
							string selectsql = "SELECT vmbillid,vpfcode,vshop,vspcode,vvipcard,fquantity,fmoney,fchange,vsposition,dtdate,bPutup,bCancel,bTotal,fRealMoney,fGetMoney,fCutMoney,vEmpCode,isonline FROM SG_Gathering";
							var table = BusinessDbUtil.GetDataTable(string.Format("{0} WHERE  vMBillID='{1}'", selectsql, billData.OrderId));
							table.TableName = "SG_Gathering";
							table.PrimaryKey = new DataColumn[] { table.Columns["vmbillid"] };
							BusinessDbUtil.DoAction(dbManager =>
							{
								string sql = string.Empty;
								try
								{
									if (table.Rows.Count < 1)
									{
										//主表
										if (!ValidDJUtil.IsExistKHDM(this, billData.OrderId, dataRow["storeid"].ToString()))
										{
											//Message = JsonHelper.SuccessXmlMsg("failure", "-1", "客户代码不存在！"); 
										}

										string strColumns = SqlUtil.GetColumns(table);
										DataRow row = table.NewRow();
										row["vmbillid"] = billData.OrderId;
										row["vpfcode"] = "0";
										row["vshop"] = KHDM;
										//营业员
										row["vspcode"] = DYDM_YYY;
										row["vvipcard"] = "";
										row["fquantity"] = -amount;
										row["fmoney"] = -billData.TotalFee/100;
										row["fchange"] = 0;
										row["vsposition"] = "000";
										row["dtdate"] = DateTime.Now.ToShortDateString();
										row["bPutup"] = 0;  //挂单
										row["bCancel"] = 0; //作废
										row["bTotal"] = 0; //汇总
										row["fRealMoney"] = -billData.TotalFee/100;
										row["fGetMoney"] = -billData.TotalFee / 100;
										row["fCutMoney"] = 0;
										row["vEmpCode"] = DYDM;
										row["isonline"] = "1";
										table.Rows.Add(row);
										string masterSql = SqlUtil.ConvertInsert(table, strColumns, row);

										//明细
										//明细
										var tablemx = BusinessDbUtil.GetDataTable(string.Format(
													@"SELECT vMBillID,vStyle,vColor,vSize,fPrice,fOriginPrice,fRealPrice,fQuantity,fMoney,fRealMoney,fRebate,vLBillID,vPFCode,vShop,vEmpCode FROM SG_Gatherings WHERE 1=0"));

										tablemx.TableName = "SG_Gatherings";
										tablemx.PrimaryKey = new DataColumn[] { tablemx.Columns["vMBillID"], tablemx.Columns["vLBillID"] };
										bool sku = false;
										for (int j = 0; j < billData.SkuInfoList.Count; j++)
										{
											var orderline = billData.SkuInfoList[j];
											//验证SKU是否存在
											//sku = ValidDJUtil.IsExistSku(this, orderInfo.orderCode, orderline.productCode, orderline.color, orderline.size);
											//if (!sku) break;
											var guige = BusinessDbUtil.GetDataTable(string.Format(@"select top 1 GG1DM,GG2DM from TMDZB where  SPTM='{0}' ", billData.SkuInfoList[i].SkuId));

											string fhColumns = SqlUtil.GetColumns(tablemx);
											DataRow fhrow = tablemx.NewRow();
											fhrow["vMBillID"] = billData.OrderId;
											fhrow["vStyle"] = orderline.ItemId;
											fhrow["vColor"] = guige.Rows[0]["GG1DM"];
											fhrow["vSize"] = guige.Rows[0]["GG2DM"];
											fhrow["fPrice"] = Convert.ToDouble(orderline.ItemPrice);
											fhrow["fOriginPrice"] = Convert.ToDouble(orderline.ItemPrice);
											fhrow["fRealPrice"] = Convert.ToDouble(orderline.ItemPrice)/100;
											fhrow["fQuantity"] = -Convert.ToDouble(orderline.Amount);
											fhrow["fMoney"] = - Convert.ToDouble(orderline.ItemPrice) / 100 * Convert.ToDouble(orderline.Amount);
											fhrow["fRealMoney"] = -Convert.ToDouble(orderline.ItemPrice) / 100 * Convert.ToDouble(orderline.Amount);
											fhrow["fRebate"] = 1;
											fhrow["vLBillID"] = i + 1;
											fhrow["vPFCode"] = "000";
											fhrow["vShop"] = KHDM;
											fhrow["vEmpCode"] = DYDM;
											tablemx.Rows.Add(fhrow);
										} 

										//结算明细
										var jsmx = BusinessDbUtil.GetDataTable(string.Format(
												   @"SELECT vLBillID,vMBillID,vShop,vBalCode,fMoney,vPFCode,vEmpCode,fChange FROM SG_PayMethod WHERE 1=0"));
										jsmx.TableName = "SG_PayMethod";
										jsmx.PrimaryKey = new DataColumn[] { jsmx.Columns["vMBillID"], jsmx.Columns["vLBillID"] };

										DataRow fhrowjs = jsmx.NewRow();
										fhrowjs["vMBillID"] = billData.OrderId;
										fhrowjs["vLBillID"] = i + 1;
										fhrowjs["vShop"] = KHDM;
										fhrowjs["vBalCode"] = "000";
										fhrowjs["fMoney"] = -billData.TotalFee/100;
										fhrowjs["vPFCode"] = 1;
										fhrowjs["vEmpCode"] = DYDM;
										fhrowjs["fChange"] = "0";
										jsmx.Rows.Add(fhrowjs);

										IEnumerable<string> sqlList = SqlUtil.Convert(tablemx);
										IEnumerable<string> sqlList2 = SqlUtil.Convert(jsmx);

										bool flag = false;
										try
										{
											LogUtil.WriteInfo(this, "", "masterSql语句为:" + masterSql + "");
											var logsql = string.Empty;
											foreach (string sqlItem in sqlList)
											{
												logsql = logsql + sqlItem + ';';
											}
											LogUtil.WriteInfo(this, "", "logsql语句为:" + logsql + "");
											foreach (string sqlItem in sqlList2)
											{
												logsql = logsql + sqlItem + ';';
											}
											LogUtil.WriteInfo(this, "", "logsql语句为:" + logsql + "");

											sql = dbManager.ExecuteBatchQuery(out flag, masterSql, sqlList, sqlList2);
										}
										catch (Exception ex)
										{
											LogUtil.WriteError(this, "sql语句执行", ex.Message);
										}
										sql = string.Format("UPDATE SG_Gathering SET fMoney=(SELECT SUM(fMoney) FROM SG_Gatherings WHERE vMBillID='{0}') WHERE vMBillID='{0}'", billData.OrderId);
										dbManager.ExecuteNonQuery(sql);

										string sql2 = "update OrderCreate set ismove='1' where cast(storeid as varchar(50))='" + dataRow["storeid"].ToString() + "' and  cast(orderId as varchar(50)) ='" + dataRow["orderId"].ToString() + "'";
										BusinessDbUtil.ExecuteNonQuery(sql2);
										//上传小票接口
										LogUtil.WriteInfo(this, "创建小票成功 小票单号 :" + billData.OrderId, "单据保存成功,对应单据编号为:" + billData.OrderId);
										
									}
									else
									{
										//LogUtil.WriteInfo(this, sRequest, "单据已存在,对应单据编号为:" + orderInfo.orderCode + "");
										//Message = JsonHelper.SuccessXmlMsg("failure", "-1", "单据已存在！");
									}
								}
								catch (Exception ex)
								{
									LogUtil.WriteError(this, sql, ex);

									//Msg = JsonHelper.SuccessXmlMsg("failure", "-1", ex.Message);
									//result = false;
								}
							});
						}
						else
						{
							LogUtil.WriteInfo(this, rsp.Body, string.Format("返回的状态：flag:{0},code:{1},message:{2}", rsp.Body, rsp.Body, rsp.Body));
						}
					}
				}
				catch (Exception ex)
				{
					LogUtil.WriteError(this, ex.Message, ex.Message);
				} 
			}
		}
		public override string BillType
		{
			get
			{
				return "消息通知-创建单据并获取明细";
			}
		}

		public override string Description
		{
			get
			{
				return "SetOrderCreateDetail";
			}
		}
	}
}
 