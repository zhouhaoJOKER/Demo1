using BSERP.Connectors.Efast.Interface;
using BSERP.Connectors.Efast.Log;
using BSERP.Connectors.Efast.Services;
using BSERP.Connectors.Efast.Util;
using ERPApiService.Business.ERPConnetionIWMS.entity;
using ERPApiService.Common.Business;
using ERPApiService.Common.helper;
using ERPApiService.Common.Object;
using HangFire.Demo1.Models.ERPConnetionForQT.entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;

namespace ERPApiService.Business.ERPConnetionForQT.set
{
    /// <summary>
    /// ID精确查询sku明细（基础信息+库存）  taobao.retail.ifashion.skuinfo.get
    /// </summary>
    public class SetSearchOnlineGoodsInfoByAccurate : StatusTask<QT_GoodsInfo>, IBillType
	{
		private readonly object Onlock = new object();
		public override string BillType
		{
			get
			{
				return "ID精确查询sku明细（基础信息+库存）";
			}
		}

		public override string Description
		{
			get
			{
				return "ID精确查询sku明细（基础信息+库存）";
			}
		}

		public override void Run()
		{
			string sql = string.Format(@"select store_id,item_id,sku_id  from ( 
			                                    select ROW_NUMBER() over(order by store_id desc) row_index ,store_id,
			                                    item_id,sku_id from storesku_mid where  
			                                    not exists(select 1 from  storeskulist where   cast(storeskulist.store_id as varchar(50))+ 
			                                    cast(storeskulist.item_id as varchar(50))+ cast(storeskulist.sku_id as varchar(50)) 
			                                    = cast(storesku_mid.store_id as varchar(50))+ cast(storesku_mid.item_id as varchar(50))
			                                    + cast(storesku_mid.sku_id as varchar(50))) ) a where row_index>=0 and row_index<=100 ");
			//string sql = "select * from storesku_mid where id ='8'";
			DataTable dataTable = BusinessDbUtil.GetDataTable(sql);
			if (dataTable != null && dataTable.Rows.Count > 0)
			{
				string app_key = ConfigUtil.App_key;
				string app_secret = ConfigUtil.App_secret;
				string iposApiUrl = ConfigUtil.IposApiUrl;
				ITopClient topClient = new DefaultTopClient(iposApiUrl, app_key, app_secret, "json");
				RetailIfashionSkuinfoGetRequest retailIfashionSkuinfoGetRequest = new RetailIfashionSkuinfoGetRequest();
				DbOperation dbOperation = new DbOperation(ConfigUtil.ConnectionString);
				string strCmd = "select ID,store_id,item_id,sku_id,sku_bar_code,shop_name,seller_nick,item_title,item_pic,item_price,color,size,short_url,current_amount from storeskulist where 1=0";
				DataTable dataTable2 = dbOperation.ExecuteQuery(strCmd).Tables[0];
				dataTable2.TableName = "storeskulist";
				foreach (DataRow dataRow in dataTable.Rows)
				{
					string accessToken_QT = InvoicesManage.GetAccessToken_QT(dataRow["store_id"].ToString());
					retailIfashionSkuinfoGetRequest.SkuId = dataRow["sku_id"].ToString();
					retailIfashionSkuinfoGetRequest.ItemId = dataRow["item_id"].ToString();
					RetailIfashionSkuinfoGetResponse retailIfashionSkuinfoGetResponse = topClient.Execute<RetailIfashionSkuinfoGetResponse>(retailIfashionSkuinfoGetRequest, accessToken_QT);
					if (retailIfashionSkuinfoGetResponse != null && retailIfashionSkuinfoGetResponse.Result.SuccessAndHasValue && retailIfashionSkuinfoGetResponse.Result.Data != null)
					{
						LogUtil.WriteInfo(this, "SetSearchOnlineGoodsInfoByAccurate : Body 成功记录", retailIfashionSkuinfoGetResponse.Body);
						RetailIfashionSkuinfoGetResponse.SkuInfoDomain data = retailIfashionSkuinfoGetResponse.Result.Data;
						DataTable dataTable3 = JsonHelper.SetDataTableFromQT<RetailIfashionSkuinfoGetResponse.SkuInfoDomain>(data, "storeskulist");
						foreach (DataRow dataRow2 in dataTable3.Rows)
						{
							string sqlFlag = string.Format(@"select top 1 id from storeskulist where cast(store_id as varchar(50)) ='{0}' and cast(item_id as varchar(50))='{1}' and cast(sku_id as varchar(50)) ='{2}'
                                             ", dataRow2["store_id"].ToString(), dataRow2["item_id"].ToString(), dataRow2["sku_id"].ToString());

							var Flag = BusinessDbUtil.ExecuteScalar(sqlFlag);
							//如果存在那就不能重复插入
							if (Flag != null && Flag.ToString() != "") { continue; }

							if (dataRow2["item_id"] != null && dataRow2["item_id"].ToString() != "" && dataRow2["color"] != null && dataRow2["color"].ToString() != "" && (dataRow2["size"] != null & dataRow2["size"].ToString() != ""))
							{
								StringBuilder stringBuilder = new StringBuilder();
								//先插入商品档案
								string str = string.Format(@"IF NOT EXISTS(SELECT 1 FROM shangpin WHERE SPDM='{0}') 
									                        BEGIN 
															   INSERT INTO shangpin(SPDM,SPMC,DWMC,fjsx1,fjsx2,fjsx3,fjsx4,fjsx5,fjsx6,fjsx7,fjsx8,fjsx9,fjsx10,BZHU,BZSJ,SJ1,SJ2,SJ3,SJ4,BZJJ,JJ1,JJ2
                                                               ,TZSY,BYZD11,BYZD1,BYZD2,BYZD12,BYZD13,BYZD9,BYZD10,JSJM,BYZD4,BYZD5,BYZD3,BZDW,BYZD14,BYZD15) 
                                                               VALUES('{0}','{1}','未定义','000','000','000','000','000','000','000','000','000','000','蜻蜓平台同步','{2}','{2}','{2}','{2}','{2}','{2}','{2}','{2}'
                                                               ,0,0,0,2,1,1,0,0,0,'000','000','000',0,GETDATE(),GETDATE())
                                                            END ", dataRow2["item_id"].ToString(), dataRow2["item_title"].ToString(), Convert.ToInt32(dataRow2["item_price"]) / 100);
								stringBuilder.Append(str + "\n");
								//颜色档案
								str = string.Format(@"IF NOT EXISTS(SELECT 1 FROM dbo.GUIGE1 WHERE GGMC='{0}')   
														 BEGIN       
														   DECLARE @ID_GUIGE1 INT = 0 
														   SELECT @ID_GUIGE1 = isnull(max(ID_VALUE),0) FROM ID_CODEID WHERE ID_NAME = 'GUIGE1_QT' 
														   IF @ID_GUIGE1 = 0  
															  BEGIN  
																insert into GUIGE1(GGDM,GGMC,TYBJ) values('QT_' + CAST(@ID_GUIGE1 AS VARCHAR(6)),'{0}',0) 
																INSERT INTO ID_CODEID (ID_NAME, ID_VALUE) VALUES ('GUIGE1_QT', 1)    
															  END  
														   ELSE
															  BEGIN
																insert into GUIGE1(GGDM,GGMC,TYBJ) values('QT_' + CAST(@ID_GUIGE1 AS VARCHAR(6)),'{0}',0)
																UPDATE ID_CODEID SET ID_VALUE = ID_VALUE + 1 WHERE ID_NAME = 'GUIGE1_QT'
															  END
														 END ", dataRow2["color"].ToString());
								stringBuilder.Append(str + "\n");
								//尺码档案
								str = string.Format(@"IF NOT EXISTS(SELECT 1 FROM dbo.GUIGE2 WHERE GGMC='{0}')
													BEGIN 
													DECLARE @GGWZ1 INT = 1,@GGWZ2 INT =1
													DECLARE @flag INT = 0
													DECLARE @ID_GUIGE2 INT = 0
													SELECT @ID_GUIGE2 = isnull(max(ID_VALUE),0) FROM ID_CODEID WHERE ID_NAME = 'GUIGE2_QT'
													IF @ID_GUIGE2 = 0
														BEGIN
														WHILE @GGWZ1 < 11
															BEGIN
															WHILE @GGWZ2 < 11
																BEGIN
																IF NOT EXISTS(SELECT 1 FROM GUIGE2 WHERE GGWZ1=@GGWZ1 AND GGWZ2=@GGWZ2)
																	BEGIN
																	SET @flag = 1
																	insert into GUIGE2(GGDM,GGMC,TYBJ,GGWZ1,GGWZ2) values('QT_' + CAST(@ID_GUIGE2 AS VARCHAR(6)),'{0}',0,@GGWZ1,@GGWZ2)
																	BREAK
																	END
																	SET @GGWZ2 = @GGWZ2 + 1   
																END
															IF @flag =1 
															BEGIN     
														BREAK                                                   
														END 
														ELSE
														BEGIN
														SET @GGWZ1 = @GGWZ1 + 1
														SET @GGWZ2 =  1
													END
													END 
														INSERT INTO ID_CODEID (ID_NAME, ID_VALUE) VALUES ('GUIGE2_QT', 1)
														END 
													ELSE
													BEGIN
														WHILE @GGWZ1<11 
															BEGIN
															WHILE @GGWZ2<11 
																BEGIN 
																IF NOT EXISTS(SELECT 1 FROM GUIGE2 WHERE GGWZ1=@GGWZ1 AND GGWZ2=@GGWZ2)
																BEGIN
																	SET @flag = 1
																	insert into GUIGE2(GGDM,GGMC,TYBJ,GGWZ1,GGWZ2) values('QT_' + CAST(@ID_GUIGE2 AS VARCHAR(6)),'{0}',0,@GGWZ1,@GGWZ2) 
																	BREAK   
																END
																SET @GGWZ2 = @GGWZ2 + 1
																END 
															IF @flag =1 
															BEGIN
																BREAK
															END  
															ELSE
																BEGIN
																SET @GGWZ1 = @GGWZ1 + 1
																SET @GGWZ2 =  1
															END 
															END 
															UPDATE ID_CODEID SET ID_VALUE = ID_VALUE + 1 WHERE ID_NAME = 'GUIGE2_QT'
															END  
													END  ", dataRow2["size"].ToString());
								stringBuilder.Append(str + "\n");

								//插入商品规则1
								str = string.Format(@" IF NOT EXISTS(SELECT 1 FROM SPGG1 INNER JOIN dbo.GUIGE1 ON SPGG1.GGDM= GUIGE1.GGDM WHERE SPDM='{0}' AND dbo.GUIGE1.GGMC='{1}')                                       
														BEGIN
                                                           DECLARE @GGDM_GUIGE1 VARCHAR(10) = ''
                                                           SELECT TOP 1 @GGDM_GUIGE1 = GGDM from GUIGE1 WHERE GGMC = '{1}'
                                                           IF @GGDM_GUIGE1 <> ''
                                                           INSERT INTO SPGG1(SPDM, GGDM, BYZD2, BYZD3) VALUES('{0}', '' + @GGDM_GUIGE1 + '', '000', '1')
							                            END ", dataRow2["item_id"].ToString(), dataRow2["color"].ToString());
								stringBuilder.Append(str + "\n");
								//插入商品规则2
								str = string.Format(@"IF NOT EXISTS(SELECT 1 FROM SPGG2 INNER JOIN dbo.GUIGE2 ON SPGG2.GGDM = GUIGE2.GGDM WHERE SPDM='{0}' AND dbo.GUIGE2.GGMC='{1}')
													BEGIN
														  DECLARE @GGDM_GUIGE2 VARCHAR(10)=''
														  SELECT TOP 1 @GGDM_GUIGE2=GGDM from GUIGE2 WHERE GGMC='{1}'
														  IF @GGDM_GUIGE2<>''
														  INSERT INTO SPGG2(SPDM,GGDM,BYZD3) VALUES('{0}',''+@GGDM_GUIGE2+'','1')
													END  ", dataRow2["item_id"].ToString(), dataRow2["size"].ToString());
								stringBuilder.Append(str + "\n");
								// 插入到TMDZB 第一次插入SPTM 针对的是 sku_id
								str = string.Format(@"IF NOT EXISTS(SELECT 1 FROM dbo.TMDZB WHERE SPTM='{0}')
													BEGIN
														DECLARE @GG1DM_TMDZB  VARCHAR(50)= ''
														DECLARE @GG2DM_TMDZB  VARCHAR(50)= ''
														SELECT TOP 1 @GG1DM_TMDZB=GGDM from dbo.GUIGE1 WHERE GGMC='{2}'
														SELECT TOP 1 @GG2DM_TMDZB=GGDM from dbo.GUIGE2 WHERE GGMC='{3}'
														INSERT INTO TMDZB(SPTM,SPDM,GG1DM,GG2DM) VALUES('{0}','{1}',''+@GG1DM_TMDZB+'',''+@GG2DM_TMDZB+'')
													END 
													", dataRow2["sku_id"].ToString(), dataRow2["item_id"].ToString(), dataRow2["color"].ToString(), dataRow2["size"].ToString());

								// 插入到TMDZB 第一次插入SPTM 针对的是 short_url
								str = string.Format(@"IF NOT EXISTS(SELECT 1 FROM dbo.TMDZB WHERE SPTM='{0}')
													BEGIN
														DECLARE @GG1DM_TMDZB  VARCHAR(50)= ''
														DECLARE @GG2DM_TMDZB  VARCHAR(50)= ''
														SELECT TOP 1 @GG1DM_TMDZB=GGDM from dbo.GUIGE1 WHERE GGMC='{2}'
														SELECT TOP 1 @GG2DM_TMDZB=GGDM from dbo.GUIGE2 WHERE GGMC='{3}'
														INSERT INTO TMDZB(SPTM,SPDM,GG1DM,GG2DM) VALUES('{0}','{1}',''+@GG1DM_TMDZB+'',''+@GG2DM_TMDZB+'')
													END 
													", dataRow["short_url"].ToString(), dataRow["item_id"].ToString(), dataRow["color"].ToString(), dataRow["size"].ToString());

								stringBuilder.Append(str + "\n");

								stringBuilder.Append(str + "\n");
								//插入客户代码
								str = string.Format(@"IF NOT EXISTS(SELECT 1 FROM kehu WHERE khdm='{0}')
														BEGIN   
														INSERT INTO KEHU(KHDM,KHMC,LBDM,QDDM,QYDM,YGDM,BYZD2,JGSD,TJSD,ZK,CKDM,XZDM,TZSY,BYZD25) 
														VALUES('{0}','{0}','000','000','000','000','1','BZSJ','BZSJ',1,'{0}','2','0',getdate())  END   ",
												dataRow2["store_id"].ToString());
								stringBuilder.Append(str + "\n");
								// 插入仓库代码
								str = string.Format(@"IF NOT EXISTS(SELECT 1 FROM dbo.CANGKU WHERE CKDM='{0}')  
                                                        BEGIN    
														INSERT INTO CANGKU(CKDM,CKMC,QDDM,YGDM,LBDM,QYDM,XZDM,DH2,JGSD,ZK,TJSD,TZSY)  
														VALUES('{0}','{0}','000','000','000','000','1','1','BZSJ',1,'BZSJ','0')  END    ",
													dataRow2["store_id"].ToString());
								// 插入仓库库位
								str = string.Format(@"
                                                    IF NOT EXISTS(SELECT 1 FROM dbo.CKKW WHERE CKDM='{0}')
                                                     BEGIN
                                                    insert into CKKW(CKDM,KWDM,INUSE,INZK,OUTUSE,OUTZK,BYZD2) values('{0}','000','0',1,'0','1',1) 
                                                     END ",
													dataRow2["store_id"].ToString());
								stringBuilder.Append(str + "\n");

								stringBuilder.Append(str + "\n");
								//插入供货商代码
								str = string.Format(@"IF NOT EXISTS(SELECT 1 FROM dbo.GONGHUOSHANG WHERE GHSMC='{0}') 
														BEGIN    
														DECLARE @ID_GONGHUOSHANG INT = 0     
														SELECT @ID_GONGHUOSHANG = isnull(max(ID_VALUE),0) FROM ID_CODEID WHERE ID_NAME = 'GONGHUOSHANG_QT'    
														IF @ID_GONGHUOSHANG = 0    
														BEGIN      
														INSERT INTO GONGHUOSHANG(GHSDM,GHSMC,XZDM,QDDM,LBDM,QYDM,YGDM,DH2,JGSD,ZK,FPLX,TZSY,CreateDate)     
														VALUES('QT_GHSDM' + CAST(@ID_GONGHUOSHANG AS VARCHAR(6)),'{0}','0','000','000','000','000','1','BZSJ','1','000','0',getdate())    
														INSERT INTO ID_CODEID (ID_NAME, ID_VALUE) VALUES ('GONGHUOSHANG_QT', 1)   
														END     
														ELSE      
														BEGIN      
														INSERT INTO GONGHUOSHANG(GHSDM,GHSMC,XZDM,QDDM,LBDM,QYDM,YGDM,DH2,JGSD,ZK,FPLX,TZSY,CreateDate)    
														VALUES('QT_GHSDM' + CAST(@ID_GONGHUOSHANG AS VARCHAR(6)),'{0}','0','000','000','000','000','1','BZSJ','1','000','0',getdate())  
														UPDATE ID_CODEID SET ID_VALUE = ID_VALUE + 1 WHERE ID_NAME = 'GONGHUOSHANG_QT'     
														END  END    ", dataRow2["shop_name"].ToString());
								stringBuilder.Append(str + "\n");

								//插入SPKCB
								//str = string.Format(@"DECLARE @GG1DM VARCHAR(50) = '',@GG2DM VARCHAR(50) ='' 
								//                            SELECT TOP 1 @GG1DM=GGDM FROM dbo.GUIGE1 WHERE GGMC='{2}' 
								//	SELECT TOP 1 @GG2DM=GGDM FROM dbo.GUIGE2 WHERE GGMC='{3}'  
								//	IF NOT EXISTS(SELECT 1 FROM dbo.SPKCB WHERE SPDM+GG1DM+GG2DM='{1}'+''+@GG1DM+''+''+@GG2DM+'' AND KWDM='000' AND CKDM='{0}')  
								//	BEGIN    
								//	INSERT INTO SPKCB(CKDM,KWDM,SPDM,GG1DM,GG2DM,SL)     VALUES('{0}','000','{1}',''+@GG1DM+'',''+@GG2DM+'','{4}')  
								//	END  
								//	ELSE 
								//	BEGIN     
								//	UPDATE SPKCB SET SL = SL + {4} WHERE CKDM = '{0}' AND SPDM='{1}' AND GG1DM = ''+@GG1DM+'' AND GG2DM =''+@GG2DM+''  END    ",
								// dataRow["store_id"].ToString(), dataRow2["item_id"].ToString(), dataRow2["color"].ToString(), dataRow2["size"].ToString(), dataRow2["current_amount"].ToString());
								//stringBuilder.Append(str + "\n"); 
								if (stringBuilder.ToString() != "")
								{
									try
									{
										dbOperation.ExecuteNonQuery(stringBuilder.ToString());
									}
									catch (Exception ex)
									{
										LogUtil.WriteError(this, "插入商品基础档单 : sql", "sql : " + stringBuilder.ToString() + "错误日志 :" + ex.Message);
									}
								}
								#region //新增库存调整单
								if (dataRow2["current_amount"].ToString() != "0")
								{
									try
									{
										string TableName = "CKTZD";
										string DJBH = "";
										var NoticesName = string.Empty;
										Dictionary<string, DataTable> dic = new Dictionary<string, DataTable>();
										Dictionary<string, DataTable> dicMX = new Dictionary<string, DataTable>();
										List<Dictionary<string, DataTable>> BusinessList = new List<Dictionary<string, DataTable>>();
										List<YanShouInfo> ListNameInfoFACHU = new List<YanShouInfo>();
										List<YanShouInfo> ListNameInfoYANSHOU = new List<YanShouInfo>();

										var exists = false;
										lock (Onlock)
										{
											if (!exists)
											{
												Regulation shopinfo = new Regulation();
												shopinfo.DM2 = dataRow2["store_id"].ToString();
												shopinfo.SHR = "QT";
												shopinfo.DM1 = "999";
												shopinfo.RQ = DateTime.Now.ToShortDateString();
												shopinfo.YDJH = dataRow2["store_id"].ToString() + dataRow2["item_id"].ToString() + dataRow2["sku_id"].ToString();
												shopinfo.BZ = "蜻蜓平台对接-库存初始化";
												shopinfo.JE = Math.Ceiling((Convert.ToDouble(dataRow2["item_price"]) / 100) * Convert.ToDouble(dataRow2["current_amount"])).ToString();
												shopinfo.SL = dataRow2["current_amount"].ToString();
												shopinfo.ZDR = "QT";
												dic = DataTableBusiness.SetBusinessDataTable<Regulation>(shopinfo, TableName, "Regulation", TableName, out DJBH);
												dicMX = DataTableBusiness.SetEntryOrderDetail_QT_2(DJBH, TableName, dataRow2, dataRow2["store_id"].ToString());
												YanShouInfo infoYS = new YanShouInfo();
												try
												{
													infoYS = InvoicesManage.GetYsInfo(DJBH, TableName, "P_API_Oper_CKTZD_SH", "QT");
												}
												catch (System.Exception ex)
												{
													LogUtil.WriteError(this, "库存调整单 执行失败P_API_Oper_CKTZD_SH ;DJBH:" + DJBH);
												}
												ListNameInfoYANSHOU.Add(infoYS);
											}
											if (dic.Count > 0 || dicMX.Count > 0)
											{
												if (dic != null && dicMX != null)
												{
													BusinessList.Add(dic);
													BusinessList.Add(dicMX);
												}
											}
											if (BusinessList.Count > 0)
											{
												var resultList = DataTableBusiness.SavaBusinessData_SqlParameter(BusinessList, ListNameInfoYANSHOU);
												if (resultList)
												{
													sql = string.Format("UPDATE " + TableName + " SET JE=(SELECT SUM(JE) FROM " + TableName + "MX WHERE DJBH='{0}')" +
													",SL=(SELECT SUM(SL) FROM  " + TableName + "MX WHERE DJBH='{0}')WHERE DJBH='{0}'", DJBH);
													BusinessDbUtil.ExecuteNonQuery(sql);
													LogUtil.WriteInfo(this, string.Format(@"ERP业务单据{0}创建成功!对应的电商系统的调整单号:{1}保存成功", DJBH, DJBH), string.Format(@"ERP业务单据{0}创建成功!对应的电商系统的调整单号:{1}保存成功", DJBH, DJBH));
												}
												else
												{
													LogUtil.WriteError(this, "仓库调整单保存失败");
												}
											}
											else
											{
												LogUtil.WriteError(this, "仓库调整单保存失败");
											}
										}
									}
									catch (Exception ex)
									{
										LogUtil.WriteError(this, "仓库调整单保存失败" + ex.Message);
									}
								}
								#endregion  
							}
							DataRow dataRow3 = dataTable2.NewRow();
							dataRow3.BeginEdit();
							foreach (DataColumn dataColumn in dataTable2.Columns)
							{
								if (dataColumn.ColumnName.ToString() != "ID")
								{
									dataRow3[dataColumn.ColumnName] = dataRow2[dataColumn.ColumnName];
								}
							}
							dataRow3.EndEdit();
							dataTable2.Rows.Add(dataRow3);
						}
					}
					else
					{
						LogUtil.WriteInfo(this, "SetSearchOnlineGoodsInfoByAccurate : Body", "SetSearchOnlineGoodsInfoByAccurate - Body :  " + retailIfashionSkuinfoGetResponse.Body);
					}
				}
				try
				{
					if (dataTable2.Rows.Count>0 && dbOperation.SqlBulkCopy(dataTable2, "storeskulist"))
					{
						LogUtil.WriteInfo(this, "新增成功", "新增商品档案成功");
					}
				}
				catch (Exception ex)
				{
					LogUtil.WriteError(this, "error:" + ex.Message);
				}
			}
		}

		public void Run_test()
		{
			string app_key = ConfigUtil.App_key;
			string app_secret = ConfigUtil.App_secret;
			string session = ConfigUtil.session;
			string iposApiUrl = ConfigUtil.IposApiUrl;
			ITopClient topClient = new DefaultTopClient(iposApiUrl, app_key, app_secret, "json");
			RetailIfashionSkuinfoGetRequest retailIfashionSkuinfoGetRequest = new RetailIfashionSkuinfoGetRequest();
			DbOperation dbOperation = new DbOperation(ConfigUtil.ConnectionString);
			string strCmd = "select ID,store_id,item_id,sku_id,sku_bar_code,shop_name,seller_nick,item_title,item_pic,item_price,color,size,short_url,current_amount from storeskulist where 1=0";
			DataTable dataTable = dbOperation.ExecuteQuery(strCmd).Tables[0];
			dataTable.TableName = "storeskulist";
			retailIfashionSkuinfoGetRequest.SkuId = "4374388018263";
			retailIfashionSkuinfoGetRequest.ItemId = "601201018038";
			RetailIfashionSkuinfoGetResponse retailIfashionSkuinfoGetResponse = topClient.Execute<RetailIfashionSkuinfoGetResponse>(retailIfashionSkuinfoGetRequest, session);
			if (retailIfashionSkuinfoGetResponse != null && retailIfashionSkuinfoGetResponse.Result.SuccessAndHasValue && retailIfashionSkuinfoGetResponse.Result.Data != null)
			{
				RetailIfashionSkuinfoGetResponse.SkuInfoDomain data = retailIfashionSkuinfoGetResponse.Result.Data;
				DataTable dataTable2 = JsonHelper.SetDataTableFromQT<RetailIfashionSkuinfoGetResponse.SkuInfoDomain>(data, "storeskulist");
				foreach (DataRow dataRow in dataTable2.Rows)
				{
					DataRow dataRow2 = dataTable.NewRow();
					dataRow2.BeginEdit();
					foreach (DataColumn dataColumn in dataTable.Columns)
					{
						if (dataColumn.ColumnName.ToString() != "ID")
						{
							dataRow2[dataColumn.ColumnName] = dataRow[dataColumn.ColumnName];
						}
					}
					dataRow2.EndEdit();
					dataTable.Rows.Add(dataRow2);
				}
			}
			try
			{
				if (dbOperation.SqlBulkCopy(dataTable, "storeskulist"))
				{
					LogUtil.WriteInfo(this, "新增成功", "新增商品档案成功");
				}
			}
			catch (Exception ex)
			{
				LogUtil.WriteError(this, "error:" + ex.Message);
			}
		}
	}
}
