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
    /// 蜻蜓平台消息 库存变更接口
    /// 商品退货单 和 商品进货单 
    /// 零售类 直接生成零售单据 然后 明细数量如果是大于等于之前的小票的数量那就打上标记 日结状态
    /// </summary>
    public class setAmountChange_FromQT : StatusTask<QT_SPKCB>, IBillType
    {
		public object synObject = new object();

		public override void Run()
		{
			string sql = @"select top 200 amount,current_amount,item_id,sku_id,store_id,sku_id,order_id,order_type from 
			                        ItemAmountChanged where ISNULL(isMove,'0')<>'1' ORDER BY lastchanged desc";
			//string sql = @"select top 200 amount,current_amount,item_id,sku_id,store_id,sku_id,order_id,order_type from 
			//                        ItemAmountChanged where order_id='908593952148647920' and id='29'  ORDER BY lastchanged desc";
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
					array[num3] = new Thread(() =>
					{
						this.setAmountChanged(drs);
					});
					array[num3].Name = "线程" + i.ToString();
					array[num3].Start();
					num3++;
					num2 += 100;
				}
			}
		}

		public void setAmountChanged(DataRow[] drs)
		{
			foreach (DataRow dr in drs) 
			{
				QT_GoodsInfo qT_GoodsInfo2 = new QT_GoodsInfo();
				qT_GoodsInfo2.amount = Convert.ToInt32(dr["amount"]);
				qT_GoodsInfo2.current_amount = (dr["current_amount"]).ToString();
				qT_GoodsInfo2.item_id = dr["item_id"].ToString();
				qT_GoodsInfo2.store_id = dr["store_id"].ToString();
				qT_GoodsInfo2.order_id = dr["order_id"].ToString();
				qT_GoodsInfo2.sku_id = dr["sku_id"].ToString();
				qT_GoodsInfo2.type = dr["order_type"].ToString();

				if (qT_GoodsInfo2.type == "qt-stock-in")
				{
					this.InsertSPJHD(qT_GoodsInfo2);
				}
				//采购退货
				else if (qT_GoodsInfo2.type == "qt-stock-back") 
				{
					this.InsertSPTHD(qT_GoodsInfo2);
				}
				//销售单据 库存变更
				else if (qT_GoodsInfo2.type == "qt-sale")
				{
					//生成零售销货单
					this.InsertLSXHD(qT_GoodsInfo2);
				}
				//销售退货单据 库存变更
				else if (qT_GoodsInfo2.type == "qt-sale-back")
				{
					//生成零售销货单
					this.InsertLSTHD(qT_GoodsInfo2);
				}
			}
		}
		/// <summary>
		/// 销售
		/// </summary>
		/// <param name="order"></param>
		/// <returns></returns>
		private bool InsertLSXHD(QT_GoodsInfo order)
		{
			bool result = false;
			string KHDM = order.store_id.ToString();
			string sql2 = "select top 1 KHDM,QDDM,CKDM,ZK,JGSD from kehu where khdm='" + KHDM + "'";
			DataTable dataTable = BusinessDbUtil.GetDataTable(sql2);
			string CKDM = dataTable.Rows[0]["CKDM"].ToString();
			string text = dataTable.Rows[0]["JGSD"].ToString();
			string QDDM = dataTable.Rows[0]["QDDM"].ToString();
			string SL = order.amount.ToString();
			var DJBH = InvoicesManage.GetNewDJBH("LSXHD", QDDM, CKDM, "");
			string arg = "SELECT djbh,ydjh,qddm,dm1,dm2,dm2_1,rq,sl,je,bz,zdr,rq_4,shr,sh,shrq,ygdm,byzd1,ll,byzd12,je_1,isonline FROM LSXHD";
			DataTable table = BusinessDbUtil.GetDataTable(string.Format("{0} WHERE  djbh='{1}'", arg, order.store_id));
			table.TableName = "LSXHD";
			table.PrimaryKey = new DataColumn[]
			{
				table.Columns["djbh"]
			};
			//取商品信息 包括价格
			string sql = string.Format(@"IF EXISTS (select 1 from TMDZB inner join sg_gatherings on TMDZB.SPDM = sg_gatherings.vstyle 
										and TMDZB.gg1dm=sg_gatherings.vcolor and TMDZB.gg2dm =sg_gatherings.vsize 
										where TMDZB.SPDM ='{0}' and TMDZB.SPTM='{1}' and sg_gatherings.vmbillid='{2}' )
										BEGIN
										  select TMDZB.SPDM,TMDZB.GG1DM,TMDZB.GG2DM,sg_gatherings.frealprice as BZSJ from TMDZB inner join sg_gatherings on TMDZB.SPDM = sg_gatherings.vstyle 
										  and TMDZB.gg1dm=sg_gatherings.vcolor and TMDZB.gg2dm =sg_gatherings.vsize 
										  where TMDZB.SPDM ='{0}' and TMDZB.SPTM='{1}' and sg_gatherings.vmbillid='{2}'
										END
										ELSE
										BEGIN
											select distinct TMDZB.SPDM,TMDZB.GG1DM,TMDZB.GG2DM,SHANGPIN.BZSJ from 
											TMDZB inner join SHANGPIN on TMDZB.SPDM=SHANGPIN.SPDM  
											where TMDZB.SPDM='{0}' and TMDZB.SPTM='{1}'
										END", order.item_id, order.sku_id,order.order_id);
			DataTable dt_SP = InvoicesManage.ExecuteQuery(sql);
			LogUtil.WriteInfo(this,"查询商品价格信息","查询价格sql"+sql +"数据结果:"+ JsonParser.ToJson(dt_SP));
			BusinessDbUtil.DoAction(delegate (DirectDbManger dbManager)
			{
				try
				{
					if (table.Rows.Count < 1)
					{
						if (ValidDJUtil.IsExistKHDM(this, order.order_id, order.store_id))
						{
							string columns = SqlUtil.GetColumns(table);
							DataRow dataRow = table.NewRow();
							dataRow["djbh"] = DJBH;
							dataRow["ydjh"] = order.order_id;
							dataRow["qddm"] = QDDM;
							dataRow["dm1"] = order.store_id;
							dataRow["dm2"] = CKDM;
							dataRow["dm2_1"] = "000";
							dataRow["rq"] = DateTime.Now.ToShortDateString();
							dataRow["bz"] = "蜻蜓平台同步";
							dataRow["sl"] = SL;
                            //商品价格信息需要取自单据创建的时候小票的价格信息
							dataRow["je"] = Convert.ToDecimal(dt_SP.Rows[0]["BZSJ"]) * Convert.ToDecimal(order.amount);
							dataRow["zdr"] = "QT";
							dataRow["rq_4"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
							dataRow["ygdm"] = "000";
							dataRow["byzd1"] = ValidDJUtil.GetJGXD(this, KHDM, "JGSD");
							dataRow["ll"] = "1";
							dataRow["byzd12"] = "1";
							dataRow["isonline"] = "1";
							dataRow["je_1"] = Convert.ToDecimal(dt_SP.Rows[0]["BZSJ"]) * Convert.ToDecimal(order.amount);
							table.Rows.Add(dataRow);
							string text2 = SqlUtil.ConvertInsert(table, columns, dataRow);
							DataTable dataTable2 = BusinessDbUtil.GetDataTable(string.Format("SELECT djbh,mibh,spdm,gg1dm,gg2dm,ckj,dj,sl,zk,bzje,je FROM LSXHDMX WHERE 1=0", new object[0]));
							dataTable2.TableName = "LSXHDMX";
							dataTable2.PrimaryKey = new DataColumn[]
							{
								dataTable2.Columns["djbh"],
								dataTable2.Columns["mibh"]
							};
							bool flag = true; 
							string columns2 = SqlUtil.GetColumns(dataTable2);
							DataRow dataRow3 = dataTable2.NewRow();
							dataRow3["djbh"] = DJBH;
							dataRow3["mibh"] = 0;
							dataRow3["spdm"] = order.item_id;
							dataRow3["gg1dm"] = dt_SP.Rows[0]["GG1DM"];
							dataRow3["gg2dm"] = dt_SP.Rows[0]["GG2DM"];
							dataRow3["dj"] = Convert.ToDecimal(dt_SP.Rows[0]["BZSJ"]);
							dataRow3["ckj"] = Convert.ToDecimal(dt_SP.Rows[0]["BZSJ"]);
							dataRow3["sl"] = order.amount;
							dataRow3["zk"] = "1";
							dataRow3["je"] = Convert.ToDecimal(dt_SP.Rows[0]["BZSJ"]) * Convert.ToDecimal(order.amount);
							dataRow3["bzje"] = Convert.ToDecimal(dt_SP.Rows[0]["BZSJ"]) * Convert.ToDecimal(order.amount);
							dataTable2.Rows.Add(dataRow3);

							if (flag)
							{
								DataTable dataTable3 = BusinessDbUtil.GetDataTable(string.Format("SELECT DJBH,mibh,JSFS,JE FROM LSXHDJS WHERE 1=0", new object[0]));
								dataTable3.TableName = "LSXHDJS";
								dataTable3.PrimaryKey = new DataColumn[]
								{
									dataTable3.Columns["DJBH"],
									dataTable3.Columns["mibh"]
								};
								DataRow dataRow4 = dataTable3.NewRow();
								dataRow4["DJBH"] = DJBH;
								dataRow4["mibh"] = 1;
								dataRow4["JSFS"] = "999";
								dataRow4["JE"] = Convert.ToDecimal(dt_SP.Rows[0]["BZSJ"]) * Convert.ToDecimal(order.amount);
								dataTable3.Rows.Add(dataRow4);
								IEnumerable<string> enumerable = SqlUtil.Convert(dataTable2);
								IEnumerable<string> enumerable2 = SqlUtil.Convert(dataTable3);
								bool flag2 = false;
								try
								{
									sql = dbManager.ExecuteBatchQuery(out flag2, text2, enumerable, enumerable2);
									InvoicesManage.UpdateBillPrice(DJBH, "LSXHD");
									DataTable procedureParameter = dbManager.GetProcedureParameter("P_LSXHDFLAGPROCESS");
									dbManager.SetProcedureParameter(procedureParameter, "DJBH", DJBH);
									dbManager.SetProcedureParameter(procedureParameter, "USER", "QT");
									dbManager.SetProcedureParameter(procedureParameter, "Flag", "1");
									dbManager.SetProcedureParameter(procedureParameter, "OpDate", DateTime.Now.ToShortDateString());
									DataTable dataTable4 = dbManager.ExecProcedure("P_LSXHDFLAGPROCESS", procedureParameter);
									DataTable procedureParameter2 = dbManager.GetProcedureParameter("P_UPDATEPTKC_1");
									dbManager.SetProcedureParameter(procedureParameter2, "ckdm", CKDM);
									dbManager.SetProcedureParameter(procedureParameter2, "kwdm", "000");
									dbManager.SetProcedureParameter(procedureParameter2, "tblName", "LSXHDMP");
									dbManager.SetProcedureParameter(procedureParameter2, "djbh", DJBH);
									dbManager.SetProcedureParameter(procedureParameter2, "intSlLx", -1);
									DataTable dataTable5 = dbManager.ExecProcedure("P_UPDATEPTKC_1", procedureParameter2);

									//更新小票日结状态
									var lsxhCounts = "select sum(lsxhdmx.sl) as sl from lsxhdmx left join lsxhd on lsxhd.djbh = lsxhdmx.djbh where lsxhd.ydjh='" + order.order_id + "'";
									DataTable lsxhCounts_Table = BusinessDbUtil.GetDataTable(lsxhCounts);

									var sg_Counts = "select sum(fquantity) as sl from sg_gatherings where vmbillid='" + order.order_id + "'";
									DataTable sg_Counts_Table = BusinessDbUtil.GetDataTable(lsxhCounts);
									if (sg_Counts_Table != null && sg_Counts_Table.Rows.Count > 0 && lsxhCounts_Table.Rows[0]["sl"] == sg_Counts_Table.Rows[0]["sl"])
									{
										//小票明细数量和
										BusinessDbUtil.GetDataTable("update sg_gathering set btotal= '1' where vmbillid ='" + order.order_id + "'");
									}

									if (flag2 && dataTable4 != null && dataTable4.Rows.Count > 0 && dataTable5 != null && dataTable5.Rows.Count > 0)
									{
										LogUtil.WriteInfo(this, "", "单据保存成功,对应单据编号为:" + DJBH);
										result = true;
									}
									else
									{
										LogUtil.WriteInfo(this, "", string.Concat(new object[]
										{
										"单据保存失败,对应sql语句为:",
										text2,
										enumerable,
										enumerable2
										}));
									}

								}
								catch (System.Exception ex)
								{
									LogUtil.WriteError(this, "", "单据保存成功,对应单据编号为:" + order.order_id);
								} 
							}
						}
					}
					else
					{
						LogUtil.WriteInfo(this, "", "单据已存在,对应单据编号为:" + order.order_id);
					}
				}
				catch (Exception ex)
				{
					LogUtil.WriteError(this, sql, ex, "");
					result = false;
				}
			}); 
			this.UpdatesetAmountChangedState(order);
			return result;
		}

		/// <summary>
		/// 销售退货
		/// </summary>
		/// <param name="order"></param>
		/// <returns></returns>
		private bool InsertLSTHD(QT_GoodsInfo order)
		{
			bool result = false;
			string KHDM = order.store_id.ToString();
			string sql2 = "select top 1 KHDM,QDDM,CKDM,ZK,JGSD from kehu where khdm='" + KHDM + "'";
			DataTable dataTable = BusinessDbUtil.GetDataTable(sql2);
			string CKDM = dataTable.Rows[0]["CKDM"].ToString();
			string text = dataTable.Rows[0]["JGSD"].ToString();
			string QDDM = dataTable.Rows[0]["QDDM"].ToString();
			string SL = order.amount.ToString();
			var DJBH = InvoicesManage.GetNewDJBH("LSTHD", QDDM, CKDM, "");
			string arg = "SELECT djbh,ydjh,qddm,dm1,dm2,dm2_1,rq,sl,je,bz,zdr,rq_4,shr,sh,shrq,ygdm,byzd1,ll,byzd12,je_1,isonline FROM LSTHD";
			DataTable table = BusinessDbUtil.GetDataTable(string.Format("{0} WHERE  djbh='{1}'", arg, order.store_id));
			table.TableName = "LSTHD";
			table.PrimaryKey = new DataColumn[]
			{
				table.Columns["djbh"]
			};
			//string sql = string.Format(@"select distinct TMDZB.SPDM,TMDZB.GG1DM,TMDZB.GG2DM,SHANGPIN.BZSJ from 
			//			TMDZB inner join SHANGPIN on TMDZB.SPDM=SHANGPIN.SPDM  
   //                     where TMDZB.SPDM='{0}' and TMDZB.SPTM='{1}' ", order.item_id, order.sku_id);

			//取商品信息 包括价格
			string sql = string.Format(@"IF EXISTS (select 1 from TMDZB inner join sg_gatherings on TMDZB.SPDM = sg_gatherings.vstyle 
										and TMDZB.gg1dm=sg_gatherings.vcolor and TMDZB.gg2dm =sg_gatherings.vsize 
										where TMDZB.SPDM ='{0}' and TMDZB.SPTM='{1}' and sg_gatherings.vmbillid='{2}' )
										BEGIN
										  select TMDZB.SPDM,TMDZB.GG1DM,TMDZB.GG2DM,sg_gatherings.frealprice as BZSJ from TMDZB inner join sg_gatherings on TMDZB.SPDM = sg_gatherings.vstyle 
										  and TMDZB.gg1dm=sg_gatherings.vcolor and TMDZB.gg2dm =sg_gatherings.vsize 
										  where TMDZB.SPDM ='{0}' and TMDZB.SPTM='{1}' and sg_gatherings.vmbillid='{2}'
										END
										ELSE
										BEGIN
											select distinct TMDZB.SPDM,TMDZB.GG1DM,TMDZB.GG2DM,SHANGPIN.BZSJ from 
											TMDZB inner join SHANGPIN on TMDZB.SPDM=SHANGPIN.SPDM  
											where TMDZB.SPDM='{0}' and TMDZB.SPTM='{1}'
										END", order.item_id, order.sku_id, order.order_id);
			DataTable dt_SP = InvoicesManage.ExecuteQuery(sql);
			BusinessDbUtil.DoAction(delegate (DirectDbManger dbManager)
			{
				try
				{
					if (table.Rows.Count < 1)
					{
						if (ValidDJUtil.IsExistKHDM(this, order.order_id, order.store_id))
						{
							string columns = SqlUtil.GetColumns(table);
							DataRow dataRow = table.NewRow();
							dataRow["djbh"] = DJBH;
							dataRow["ydjh"] = order.order_id;
							dataRow["qddm"] = QDDM;
							dataRow["dm1"] = order.store_id;
							dataRow["dm2"] = CKDM;
							dataRow["dm2_1"] = "000";
							dataRow["rq"] = DateTime.Now.ToShortDateString();
							dataRow["bz"] = "蜻蜓平台同步";
							dataRow["sl"] = SL;
							dataRow["je"] = order;
							dataRow["zdr"] = "QT";
							dataRow["rq_4"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
							dataRow["ygdm"] = "000";
							dataRow["byzd1"] = ValidDJUtil.GetJGXD(this, KHDM, "JGSD");
							dataRow["ll"] = "1";
							dataRow["byzd12"] = "1";
							dataRow["isonline"] = "1";
							dataRow["je_1"] = Convert.ToDecimal(dt_SP.Rows[0]["BZSJ"]) * Convert.ToDecimal(order.amount);
							table.Rows.Add(dataRow);
							string text2 = SqlUtil.ConvertInsert(table, columns, dataRow);
							DataTable dataTable2 = BusinessDbUtil.GetDataTable(string.Format("SELECT djbh,mibh,spdm,gg1dm,gg2dm,ckj,dj,sl,zk,bzje,je FROM LSTHDMX WHERE 1=0", new object[0]));
							dataTable2.TableName = "LSTHDMX";
							dataTable2.PrimaryKey = new DataColumn[]
							{
								dataTable2.Columns["djbh"],
								dataTable2.Columns["mibh"]
							};
							bool flag = true;
							//flag = ValidDJUtil.IsExistSku(this, order.orderId, orderline.itemId, dataRow2["GG1DM"].ToString(), dataRow2["GG2DM"].ToString());
							//if (!flag)
							//{
							//	break;
							//}
							string columns2 = SqlUtil.GetColumns(dataTable2);
							DataRow dataRow3 = dataTable2.NewRow();
							dataRow3["djbh"] = DJBH;
							dataRow3["mibh"] = 0;
							dataRow3["spdm"] = order.item_id;
							dataRow3["gg1dm"] = dt_SP.Rows[0]["GG1DM"];
							dataRow3["gg2dm"] = dt_SP.Rows[0]["GG1DM"];
							dataRow3["dj"] = Convert.ToDecimal(dt_SP.Rows[0]["BZSJ"]);
							dataRow3["ckj"] = Convert.ToDecimal(dt_SP.Rows[0]["BZSJ"]);
							dataRow3["sl"] = order.amount;
							dataRow3["zk"] = "1";
							dataRow3["je"] = Convert.ToDecimal(dt_SP.Rows[0]["BZSJ"]) * Convert.ToDecimal(order.amount);
							dataRow3["bzje"] = Convert.ToDecimal(dt_SP.Rows[0]["BZSJ"]) * Convert.ToDecimal(order.amount);
							dataTable2.Rows.Add(dataRow3);

							if (flag)
							{
								DataTable dataTable3 = BusinessDbUtil.GetDataTable(string.Format("SELECT DJBH,mibh,JSFS,JE FROM LSXHDJS WHERE 1=0", new object[0]));
								dataTable3.TableName = "LSTHDJS";
								dataTable3.PrimaryKey = new DataColumn[]
								{
									dataTable3.Columns["DJBH"],
									dataTable3.Columns["mibh"]
								};
								DataRow dataRow4 = dataTable3.NewRow();
								dataRow4["DJBH"] = DJBH;
								dataRow4["mibh"] = 1;
								dataRow4["JSFS"] = "999";
								dataRow4["JE"] = Convert.ToDecimal(dt_SP.Rows[0]["BZSJ"]) * Convert.ToDecimal(order.amount);
								dataTable3.Rows.Add(dataRow4);
								IEnumerable<string> enumerable = SqlUtil.Convert(dataTable2);
								IEnumerable<string> enumerable2 = SqlUtil.Convert(dataTable3);
								bool flag2 = false;
								try
								{
									sql = dbManager.ExecuteBatchQuery(out flag2, text2, enumerable, enumerable2);
									InvoicesManage.UpdateBillPrice(DJBH, "LSTHD");
									DataTable procedureParameter = dbManager.GetProcedureParameter("P_LSTHDFLAGPROCESS");
									dbManager.SetProcedureParameter(procedureParameter, "DJBH", DJBH);
									dbManager.SetProcedureParameter(procedureParameter, "USER", "QT");
									dbManager.SetProcedureParameter(procedureParameter, "Flag", "1");
									dbManager.SetProcedureParameter(procedureParameter, "OpDate", DateTime.Now.ToShortDateString());
									DataTable dataTable4 = dbManager.ExecProcedure("P_LSTHDFLAGPROCESS", procedureParameter);
									DataTable procedureParameter2 = dbManager.GetProcedureParameter("P_UPDATEPTKC_1");
									dbManager.SetProcedureParameter(procedureParameter2, "ckdm", CKDM);
									dbManager.SetProcedureParameter(procedureParameter2, "kwdm", "000");
									dbManager.SetProcedureParameter(procedureParameter2, "tblName", "LSTHDMP");
									dbManager.SetProcedureParameter(procedureParameter2, "djbh", DJBH);
									dbManager.SetProcedureParameter(procedureParameter2, "intSlLx", -1);
									DataTable dataTable5 = dbManager.ExecProcedure("P_UPDATEPTKC_1", procedureParameter2);

									//更新小票日结状态
									var lsxhCounts = "select sum(lsxhdmx.sl) as c from LSTHDMX left join LSTHD on LSTHD.djbh = LSTHDMX.djbh where LSTHD.ydjh='" + order.order_id + "'";
									DataTable lsxhCounts_Table = BusinessDbUtil.GetDataTable(lsxhCounts);

									var sg_Counts = "select sum(fquantity) as sl from sg_gatherings where vmbillid='" + order.order_id + "'";
									DataTable sg_Counts_Table = BusinessDbUtil.GetDataTable(lsxhCounts);
									if (sg_Counts_Table != null && sg_Counts_Table.Rows.Count > 0 && lsxhCounts_Table.Rows[0]["sl"] == sg_Counts_Table.Rows[0]["sl"])
									{
										//小票明细数量和
										BusinessDbUtil.GetDataTable("update sg_gathering set btotal= '1' where vmbillid ='" + order.order_id + "'");
									}
									if (flag2 && dataTable4 != null && dataTable4.Rows.Count > 0 && dataTable5 != null && dataTable5.Rows.Count > 0)
									{
										LogUtil.WriteInfo(this, "", "单据保存成功,对应单据编号为:" + DJBH);
										result = true;
									}
									else
									{
										LogUtil.WriteInfo(this, "", string.Concat(new object[]
										{
										"单据保存失败,对应sql语句为:",
										text2,
										enumerable,
										enumerable2
										}));
									}

								}
								catch (System.Exception ex)
								{
									LogUtil.WriteError(this, "", "单据保存成功,对应单据编号为:" + order.order_id);
								}


							}
						}
					}
					else
					{
						LogUtil.WriteInfo(this, "", "单据已存在,对应单据编号为:" + order.order_id);
					}
				}
				catch (Exception ex)
				{
					LogUtil.WriteError(this, sql, ex, "");
					result = false;
				}
			});
			this.UpdatesetAmountChangedState(order);
			return result;
		}

		/// <summary>
		/// 采购入库
		/// </summary>
		/// <param name="qT_GoodsInfo2"></param>
		private void InsertSPJHD(QT_GoodsInfo qT_GoodsInfo2)
		{
			string text2 = "SPJHD";
			string empty = string.Empty;
			Dictionary<string, DataTable> dictionary = new Dictionary<string, DataTable>();
			Dictionary<string, DataTable> dictionary2 = new Dictionary<string, DataTable>();
			List<Dictionary<string, DataTable>> list = new List<Dictionary<string, DataTable>>();
			List<YanShouInfo> list2 = new List<YanShouInfo>();
			string sql = "select top 1 KHDM,QDDM,CKDM,ZK,JGSD from kehu  where khdm ='" + qT_GoodsInfo2.store_id + "'";
			DataTable dataTable = BusinessDbUtil.GetDataTable(sql);
			string zPSD = dataTable.Rows[0]["KHDM"].ToString();
			string dM = dataTable.Rows[0]["CKDM"].ToString();
			string text3 = dataTable.Rows[0]["JGSD"].ToString();
			string qDDM = dataTable.Rows[0]["QDDM"].ToString();
			Purchase purchase = new Purchase();
			purchase.QDDM = qDDM;
			//获取供货商代码
			string sql3 = string.Format(@"select top 1 GHSDM from GONGHUOSHANG where GHSMC in (select top  1 shop_name from storeskulist
                                                  where store_id='{0}' and item_id='{1}' and sku_id='{2}')", zPSD, qT_GoodsInfo2.item_id, qT_GoodsInfo2.sku_id);

			string dM2 = BusinessDbUtil.ExecuteScalar(sql3).ToString();
			purchase.DM1 = dM2;
			//添加虚拟总仓
			purchase.DM2 = "QT_XNZC";
			purchase.DM2_1 = "000";
			purchase.LXDJ = "";
			purchase.DM4 = "ZP";
			purchase.QYDM = "000";
			purchase.BYZD1 = "0";
			purchase.FPLX = "3";
			purchase.YGDM = "000";
			purchase.BYZD3 = "";
			purchase.BYZD12 = "1";
			purchase.YGDM = "000";
			purchase.BZ = "蜻蜓平台同步";
			purchase.isonline = "1";
			//他们那边的消息是拆分的 
			purchase.DJBH = InvoicesManage.GetNewDJBH(text2, purchase.QDDM, purchase.DM1, qT_GoodsInfo2.order_id);
			purchase.ZPSD = zPSD;
			string dJBH = purchase.DJBH;
			purchase.SHR = "QT";
			purchase.ZDR = "QT";
			purchase.RQ = DateTime.Now.ToString("yyyy-MM-dd");
			purchase.YDJH = qT_GoodsInfo2.order_id;
			YanShouInfo infoYS = new YanShouInfo();
			infoYS.DJBH = purchase.DJBH;
			infoYS.TableName = text2;
			infoYS.User = "QT";
			infoYS.Procedure = string.Format("P_API_Oper_{0}_SH", text2);//验收
			infoYS.BYZD3 = "";
			list2.Add(infoYS);

			dictionary = DataTableBusiness.SetBusinessDataTable<Purchase>(purchase, text2, "Purchase", text2);
			dictionary2 = DataTableBusiness.SetEntryOrderDetail_QT_1(purchase.DJBH, text2, qT_GoodsInfo2, purchase.DM1);
			if (dictionary.Count > 0 && dictionary2.Count > 0)
			{
				list.Add(dictionary);
				list.Add(dictionary2);
			}
			if (list.Count > 0)
			{
				try
				{
					bool flag2 = DataTableBusiness.SavaBusinessData_SqlParameter(list, list2);
                    if (flag2)
                    {
                        sql = "update SPJHD set SPJHD.JE = (select SUM(JE) from SPJHDMX where SPJHDMX.DJBH='" + dJBH + "'),SPJHD.SL = (select SUM(SL) from SPJHDMX where SPJHDMX.DJBH='" + dJBH + "') where SPJHD.DJBH='" + dJBH + "'";
                        BusinessDbUtil.ExecuteNonQuery(sql);
                    }
					this.UpdatesetAmountChangedState(qT_GoodsInfo2);
					LogUtil.WriteInfo(this, "success", "SPJHD 创建成功DJBH" + dJBH);
					// 生成商店进货单
					Dictionary<string, DataTable> dic = new Dictionary<string, DataTable>();
					Dictionary<string, DataTable> dicMX = new Dictionary<string, DataTable>();
					List<YanShouInfo> ListNameInfoFACHU = new List<YanShouInfo>();
					List<YanShouInfo> ListNameInfoYANSHOU = new List<YanShouInfo>();
					List<Dictionary<string, DataTable>> BusinessList = new List<Dictionary<string, DataTable>>();
					var DJBH = string.Empty;
					var NewTableName = string.Empty;

					shopinfo ShopInfo = new shopinfo();
					ShopInfo.YDJH = dJBH;
					ShopInfo.DM1 = qT_GoodsInfo2.store_id;
					//添加虚拟总仓
					ShopInfo.DM2 = "QT_XNZC";
					ShopInfo.RQ = DateTime.Now.ToString("yyyy-MM-dd");
					//ShopInfo.DM5 = orderInfo.brandID;
					ShopInfo.DM4 = "ZP";
					ShopInfo.DM2_1 = "000";
					ShopInfo.ZDR = "QT";
					ShopInfo.SL = qT_GoodsInfo2.amount.ToString();
					ShopInfo.JE = "0";
					ShopInfo.BZ = "蜻蜓平台同步";
					bool exists = false;
					if (!exists)
					{
						dic = DataTableBusiness.SetBusinessDataTable<shopinfo>(ShopInfo, "SDPHD", "shopinfo", "SDPHD", out DJBH);
						ShopInfo.DJBH = DJBH;
						dicMX = DataTableBusiness.SetEntryOrderDetail_QT_1(ShopInfo.DJBH, "SDPHD", qT_GoodsInfo2, ShopInfo.DM1);
						if (dic.Count > 0 && dicMX.Count > 0)
						{
							BusinessList.Add(dic);
							BusinessList.Add(dicMX);
						}
						infoYS = new YanShouInfo();
						var infoFC = new YanShouInfo();
						infoYS = InvoicesManage.GetYsInfo(DJBH, "SDPHD", "P_API_Oper_SDPHD_SH", "QT");
						infoFC = InvoicesManage.GetYsInfo(DJBH, "SDPHD", "P_API_Oper_SDPHD_YS", "QT");
						ListNameInfoYANSHOU.Add(infoFC);
						ListNameInfoYANSHOU.Add(infoYS);
						try
						{
							var resultList = DataTableBusiness.SavaBusinessData_SqlParameter(BusinessList, ListNameInfoYANSHOU);
							if (resultList)
							{
								LogUtil.WriteInfo(this, "商店配货单保存成功", "DJBH:" + DJBH);
                                sql = "update SDPHD set  SDPHD.JE = (select SUM(JE) from SDPHDMX where SDPHDMX.DJBH='" + DJBH + "'),SDPHD.SL = (select SUM(SL) from SDPHDMX where SDPHDMX.DJBH='" + DJBH + "') where SDPHD.DJBH='" + DJBH + "'";
								BusinessDbUtil.ExecuteNonQuery(sql);
							}
						}
						catch (System.Exception ex)
						{
							LogUtil.WriteError(this, "直配流程商店配货单保存失败", "DJBH:" + DJBH + "message:" + ex.Message);
						}
					}
					else
					{
						//result = JsonHelper.SuccessXmlMsg("failure", "-1", string.Format("{0}保存单据失败,商店退货单据重复！", orderInfo.orderId));
						LogUtil.WriteError(this, "直配流程商店配货单保存失败", "");
					}

				}
				catch (Exception ex)
				{
					LogUtil.WriteError(this, "failure", string.Format("InsertSPJHD .保存单据失败，请检查系统日志！ {0} + BusinessList : {1}", qT_GoodsInfo2.order_id, JsonParser.ToJson(list)));
				}
			}
			else
			{
				LogUtil.WriteError(this, "failure", string.Format("保存单据失败,无保存数据,请检查系统日志！", new object[0]));
			}
		}

		/// <summary>
		/// 采购退货
		/// </summary>
		/// <param name="qT_GoodsInfo2"></param>
		private void InsertSPTHD(QT_GoodsInfo qT_GoodsInfo2)
		{
			string text2 = "SPTHD";
			string empty = string.Empty;
			Dictionary<string, DataTable> dictionary = new Dictionary<string, DataTable>();
			Dictionary<string, DataTable> dictionary2 = new Dictionary<string, DataTable>();
			List<Dictionary<string, DataTable>> list = new List<Dictionary<string, DataTable>>();
			List<YanShouInfo> list2 = new List<YanShouInfo>();
			string sql = "select top 1 KHDM,QDDM,CKDM,ZK,JGSD from kehu  where khdm ='" + qT_GoodsInfo2.store_id + "'";
			DataTable dataTable = BusinessDbUtil.GetDataTable(sql);
			string zPSD = dataTable.Rows[0]["KHDM"].ToString();
			string dM = dataTable.Rows[0]["CKDM"].ToString();
			string text3 = dataTable.Rows[0]["JGSD"].ToString();
			string qDDM = dataTable.Rows[0]["QDDM"].ToString();
			Purchase purchase = new Purchase();
			purchase.QDDM = qDDM;

			//获取供货商代码
			string sql3 = string.Format(@"select top 1 GHSDM from GONGHUOSHANG where GHSMC in (select top  1 shop_name from storeskulist
                                                  where store_id='{0}' and item_id='{1}' and sku_id='{2}')", zPSD, qT_GoodsInfo2.item_id, qT_GoodsInfo2.sku_id);

			string dM2 = BusinessDbUtil.ExecuteScalar(sql3).ToString();
			purchase.DM1 = dM2;
			//添加虚拟总仓
			purchase.DM2 = "QT_XNZC";
			purchase.DM2_1 = "000";
			purchase.LXDJ = "";
			//直配的流程
			purchase.DM4 = "ZP";
			purchase.QYDM = "000";
			purchase.BYZD1 = "0";
			purchase.FPLX = "3";
			purchase.YGDM = "000";
			purchase.BYZD3 = "";
			purchase.BYZD12 = "1";
			purchase.YGDM = "000";
			purchase.isonline = "1";
			//他们那边的消息是拆分的 
			purchase.DJBH = InvoicesManage.GetNewDJBH(text2, purchase.QDDM, purchase.DM1, qT_GoodsInfo2.order_id);
			purchase.ZPSD = zPSD;
			string dJBH = purchase.DJBH;
			purchase.SHR = "QT";
			purchase.ZDR = "QT";
			purchase.BZ = "蜻蜓平台同步";
			purchase.RQ = DateTime.Now.ToString("yyyy-MM-dd");
			purchase.YDJH = qT_GoodsInfo2.order_id;
			YanShouInfo infoYS = new YanShouInfo();
			infoYS.DJBH = purchase.DJBH;
			infoYS.TableName = text2;
			infoYS.User = "QT";
			infoYS.Procedure = string.Format("P_API_Oper_{0}_SH", text2);//验收
			infoYS.BYZD3 = "";
			list2.Add(infoYS);

			dictionary = DataTableBusiness.SetBusinessDataTable<Purchase>(purchase, text2, "Purchase", text2);
			dictionary2 = DataTableBusiness.SetEntryOrderDetail_QT_1(purchase.DJBH, text2, qT_GoodsInfo2, purchase.DM1);
			if (dictionary.Count > 0 && dictionary2.Count > 0)
			{
				list.Add(dictionary);
				list.Add(dictionary2);
			}
			if (list.Count > 0)
			{
				try
				{
					bool flag2 = DataTableBusiness.SavaBusinessData_SqlParameter(list, list2);
                    if (flag2)
                    {
                        sql = "update SPTHD set SPTHD.JE = (select SUM(JE) from SPTHDMX where SPTHDMX.DJBH='" + dJBH + "'),SPTHD.SL = (select SUM(SL) from SPTHDMX where SPTHDMX.DJBH='" + dJBH + "') where SPTHD.DJBH='" + dJBH + "'";
                        BusinessDbUtil.ExecuteNonQuery(sql);
                    }
					this.UpdatesetAmountChangedState(qT_GoodsInfo2);
					LogUtil.WriteInfo(this, "success","SPTHD 创建成功DJBH"+dJBH);
					// 生成商店退货单
					Dictionary<string, DataTable> dic = new Dictionary<string, DataTable>();
					Dictionary<string, DataTable> dicMX = new Dictionary<string, DataTable>();
					List<YanShouInfo> ListNameInfoFACHU = new List<YanShouInfo>();
					List<YanShouInfo> ListNameInfoYANSHOU = new List<YanShouInfo>();
					List<Dictionary<string, DataTable>> BusinessList = new List<Dictionary<string, DataTable>>();
					var DJBH = string.Empty;
					var NewTableName = string.Empty;

					shopinfo ShopInfo = new shopinfo();
					ShopInfo.YDJH = dJBH;
					ShopInfo.DM1 = qT_GoodsInfo2.store_id;
					//添加虚拟总仓
					ShopInfo.DM2 = "QT_XNZC";
					ShopInfo.RQ = DateTime.Now.ToString("yyyy-MM-dd");
					//ShopInfo.DM5 = orderInfo.brandID;
					ShopInfo.DM4 = "ZP";
					ShopInfo.DM2_1 = "000";
					ShopInfo.ZDR = "QT";
					ShopInfo.SL = qT_GoodsInfo2.amount.ToString();
					ShopInfo.JE = "0";
					ShopInfo.BZ = "蜻蜓平台同步";
					bool exists = false;
					if (!exists)
					{
						dic = DataTableBusiness.SetBusinessDataTable<shopinfo>(ShopInfo, "SDTHD", "shopinfo", "SDTHD", out DJBH);
						ShopInfo.DJBH = DJBH;
						dicMX = DataTableBusiness.SetEntryOrderDetail_QT_1(ShopInfo.DJBH, "SDTHD", qT_GoodsInfo2, ShopInfo.DM1);
						if (dic.Count > 0 && dicMX.Count > 0)
						{
							BusinessList.Add(dic);
							BusinessList.Add(dicMX); 
						}
						infoYS = new YanShouInfo();
						var infoFC = new YanShouInfo();
						infoYS = InvoicesManage.GetYsInfo(DJBH, "SDTHD", "P_API_Oper_SDTHD_SH", "QT");
						infoFC = InvoicesManage.GetYsInfo(DJBH, "SDTHD", "P_API_Oper_SDTHD_YS", "QT"); 
						ListNameInfoYANSHOU.Add(infoFC);
						ListNameInfoYANSHOU.Add(infoYS);
						try
						{
							var resultList = DataTableBusiness.SavaBusinessData_SqlParameter(BusinessList, ListNameInfoYANSHOU);
							if (resultList) 
							{
								LogUtil.WriteInfo(this,"商店退货单保存成功","DJBH:"+DJBH);
                                sql = "update SDTHD set  SDTHD.JE = (select SUM(JE) from SDTHDMX where SDTHDMX.DJBH='" + DJBH + "'),SDTHD.SL = (select SUM(SL) from SDTHDMX where SDTHDMX.DJBH='" + DJBH + "') where SDTHD.DJBH='" + DJBH + "'";
								BusinessDbUtil.ExecuteNonQuery(sql);
							}
						}
						catch (System.Exception ex) 
						{
							LogUtil.WriteError(this, "直配流程商店退货单保存失败", "DJBH:" +DJBH +"message:" +ex.Message);
						}
					}
					else
					{
						//result = JsonHelper.SuccessXmlMsg("failure", "-1", string.Format("{0}保存单据失败,商店退货单据重复！", orderInfo.orderId));
						LogUtil.WriteError(this, "直配流程商店退货单保存失败", ""); 
					}
				}
				catch (Exception ex)
				{
					LogUtil.WriteError(this, "failure", string.Format("InsertSPTHD 保存单据失败，请检查系统日志！ {0} + BusinessList : {1}", qT_GoodsInfo2.order_id, JsonParser.ToJson(list)));
				}
			}
			else
			{
				LogUtil.WriteError(this, "failure", string.Format("保存单据失败,无保存数据,请检查系统日志！", new object[0]));
			}
		}

		/// <summary>
		/// 更新状态 已经同步成功
		/// </summary>
		/// <param name="qT_GoodsInfo2"></param>
		private void UpdatesetAmountChangedState(QT_GoodsInfo qT_GoodsInfo2) 
		{
			try
			{
				string sqlUpdate = string.Format(@"update ItemAmountChanged set isMove = '1' 
                              where item_id+sku_id+store_id+order_id ='{0}'", qT_GoodsInfo2.item_id + qT_GoodsInfo2.sku_id +
								  qT_GoodsInfo2.store_id + qT_GoodsInfo2.order_id);

				BusinessDbUtil.ExecuteNonQuery(sqlUpdate);
			}
			catch (Exception ex) 
			{
				LogUtil.WriteError(this,"更新失败:UpdatesetAmountChangedState" + JsonParser.ToJson(qT_GoodsInfo2) );
			}
		}

		public override string BillType
		{
			get
			{
				return "蜻蜓平台消息-库存变更接口";
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
