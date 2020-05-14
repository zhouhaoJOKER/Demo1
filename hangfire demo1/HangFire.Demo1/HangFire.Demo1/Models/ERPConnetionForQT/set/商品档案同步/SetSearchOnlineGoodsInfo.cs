using HangFire.Demo1.Models.commom;
using HangFire.Demo1.Models.Entities;
using HangFire.Demo1.Models.ERPConnetionForQT.entity;
using HangFire.Demo1.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;

namespace HangFire.Demo1.Models.ERPConnetionForQT.set
{
	/// <summary>
	/// 分⻚查询⻔店sku列表  taobao.retail.ifashion.skuinfo.list
	/// </summary>
	public class SetSearchOnlineGoodsInfo : IBillType
	{
		private readonly object Onlock = new object();
		private readonly IOfficalDbManager officalDbManager;
		private readonly ITestDbManager testDbManager;
		private readonly ConfigUtil configUtil;

		public SetSearchOnlineGoodsInfo(
			IOfficalDbManager officalDbManager,
			ITestDbManager testDbManager,
			ConfigUtil configUtil)
		{
			this.officalDbManager = officalDbManager;
			this.testDbManager = testDbManager;
			this.configUtil = configUtil;
		}

		public void Run()
		{
			var userAccessToken_QT = this.officalDbManager.GetUserAccessToken_QT().ToList();
			LogUtil.WriteInfo(this, "masterTable_Token : counts", "counts : " + userAccessToken_QT.Count);
			foreach (AuthorizationToken_QT current_Token in userAccessToken_QT)
			{
				string pageSize = "50";
				string text = "select top 1 previewId,nextId from storesku where taobao_user_nick='" + current_Token.taobao_user_nick.ToString() + "' order by ID desc";
				LogUtil.WriteInfo(this, "SetSearchOnlineGoodsInfo : 获取上次同步的日期", "sql : " + text);
				DataTable dataTable = this.testDbManager.FillData(text);
				LogUtil.WriteError(this, "SetSearchOnlineGoodsInfo : masterTable", "sql : " + text);
				string app_key = configUtil.App_key;
				string app_secret = configUtil.App_secret;
				string session = current_Token.access_token.ToString();
				string iposApiUrl = configUtil.IposApiUrl;
				ITopClient topClient = new DefaultTopClient(iposApiUrl, app_key, app_secret, "json");
				string text2 = "0";
				string text3 = "0";
				if (dataTable != null && dataTable.Rows.Count > 0)
				{
					text2 = dataTable.Rows[0]["nextId"].ToString();
					text3 = text2;
				}
				RetailIfashionSkuinfoListRequest retailIfashionSkuinfoListRequest = new RetailIfashionSkuinfoListRequest();
				retailIfashionSkuinfoListRequest.Start = text2;
				retailIfashionSkuinfoListRequest.PageSize = pageSize;
				try
				{
					RetailIfashionSkuinfoListResponse retailIfashionSkuinfoListResponse = topClient.Execute<RetailIfashionSkuinfoListResponse>(retailIfashionSkuinfoListRequest, session);
					if (retailIfashionSkuinfoListResponse != null && retailIfashionSkuinfoListResponse.Result != null && retailIfashionSkuinfoListResponse.Result.SuccessAndHasValue && retailIfashionSkuinfoListResponse.Result.Data.SkuInfoList != null && retailIfashionSkuinfoListResponse.Result.Data.SkuInfoList.Count > 0)
					{
						LogUtil.WriteInfo(this, "SetSearchOnlineGoodsInfo : Body 成功记录", retailIfashionSkuinfoListResponse.Body);
						List<RetailIfashionSkuinfoListResponse.SkuInfoDomain> skuInfoList = retailIfashionSkuinfoListResponse.Result.Data.SkuInfoList;
						text3 = retailIfashionSkuinfoListResponse.Result.Data.NextId.ToString();
						DataTable dataTable2 = JsonHelper.SetDataTableFromQT<RetailIfashionSkuinfoListResponse.SkuInfoDomain>(skuInfoList, "storeskulist");
					 
						string strCmd = "select ID,store_id,item_id,sku_id,sku_bar_code,shop_name,seller_nick,item_title,item_pic,item_price,color,size,short_url,current_amount from storeskulist where 1=0";
						DataTable dataTable3 = testDbManager.FillData(strCmd);
						dataTable3.TableName = "storeskulist";
						foreach (DataRow dataRow in dataTable2.Rows)
						{
							string sqlFlag = string.Format(@"select top 1 id from storeskulist where cast(store_id as varchar(50)) ='{0}' and cast(item_id as varchar(50))='{1}' and cast(sku_id as varchar(50)) ='{2}'
                                             ", dataRow["store_id"].ToString(), dataRow["item_id"].ToString(), dataRow["sku_id"].ToString());

							var Flag = testDbManager.ExecuteScalar(sqlFlag);
							//如果存在那就不能重复插入
							if (Flag != null && Flag.ToString() != "") { continue; }
							if (dataRow["item_id"] != null && dataRow["item_id"].ToString() != "" && dataRow["color"] != null && dataRow["color"].ToString() != "" && dataRow["size"] != null & dataRow["size"].ToString() != "")
							{
								StringBuilder stringBuilder = new StringBuilder();
								//先插入商品档案
								string str = string.Format(@"IF NOT EXISTS(SELECT 1 FROM shangpin WHERE SPDM='{0}') 
									                        BEGIN 
															   INSERT INTO shangpin(SPDM,SPMC,DWMC,fjsx1,fjsx2,fjsx3,fjsx4,fjsx5,fjsx6,fjsx7,fjsx8,fjsx9,fjsx10,BZHU,BZSJ,SJ1,SJ2,SJ3,SJ4,BZJJ,JJ1,JJ2
                                                               ,TZSY,BYZD11,BYZD1,BYZD2,BYZD12,BYZD13,BYZD9,BYZD10,JSJM,BYZD4,BYZD5,BYZD3,BZDW,BYZD14,BYZD15) 
                                                               VALUES('{0}','{1}','未定义','000','000','000','000','000','000','000','000','000','000','蜻蜓平台同步','{2}','{2}','{2}','{2}','{2}','{2}','{2}','{2}'
                                                               ,0,0,0,2,1,1,0,0,0,'000','000','000',0,GETDATE(),GETDATE())
                                                            END ", dataRow["item_id"].ToString(), dataRow["item_title"].ToString(), Convert.ToInt32(dataRow["item_price"]) / 100);
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
														 END ", dataRow["color"].ToString());
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
													END  ", dataRow["size"].ToString());
								stringBuilder.Append(str + "\n");

								//插入商品规则1
								str = string.Format(@" IF NOT EXISTS(SELECT 1 FROM SPGG1 INNER JOIN dbo.GUIGE1 ON SPGG1.GGDM= GUIGE1.GGDM WHERE SPDM='{0}' AND dbo.GUIGE1.GGMC='{1}')                                       
														BEGIN
                                                           DECLARE @GGDM_GUIGE1 VARCHAR(10) = ''
                                                           SELECT TOP 1 @GGDM_GUIGE1 = GGDM from GUIGE1 WHERE GGMC = '{1}'
                                                           IF @GGDM_GUIGE1 <> ''
                                                           INSERT INTO SPGG1(SPDM, GGDM, BYZD2, BYZD3) VALUES('{0}', '' + @GGDM_GUIGE1 + '', '000', '1')
							                            END ", dataRow["item_id"].ToString(), dataRow["color"].ToString());
								stringBuilder.Append(str + "\n");
								//插入商品规则2
								str = string.Format(@"IF NOT EXISTS(SELECT 1 FROM SPGG2 INNER JOIN dbo.GUIGE2 ON SPGG2.GGDM = GUIGE2.GGDM WHERE SPDM='{0}' AND dbo.GUIGE2.GGMC='{1}')
													BEGIN
														  DECLARE @GGDM_GUIGE2 VARCHAR(10)=''
														  SELECT TOP 1 @GGDM_GUIGE2=GGDM from GUIGE2 WHERE GGMC='{1}'
														  IF @GGDM_GUIGE2<>''
														  INSERT INTO SPGG2(SPDM,GGDM,BYZD3) VALUES('{0}',''+@GGDM_GUIGE2+'','1')
													END  ", dataRow["item_id"].ToString(), dataRow["size"].ToString());
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
													", dataRow["sku_id"].ToString(), dataRow["item_id"].ToString(), dataRow["color"].ToString(), dataRow["size"].ToString());

								stringBuilder.Append(str + "\n");
								// 插入到TMDZB 第二次插入SPTM 针对的是 short_url
								str = string.Format(@"IF NOT EXISTS(SELECT 1 FROM dbo.TMDZB WHERE SPTM='{0}')
													BEGIN
														DECLARE @GG1DM_TMDZB_SHORT  VARCHAR(50)= ''
														DECLARE @GG2DM_TMDZB_SHORT  VARCHAR(50)= ''
														SELECT TOP 1 @GG1DM_TMDZB_SHORT=GGDM from dbo.GUIGE1 WHERE GGMC='{2}'
														SELECT TOP 1 @GG2DM_TMDZB_SHORT=GGDM from dbo.GUIGE2 WHERE GGMC='{3}'
														INSERT INTO TMDZB(SPTM,SPDM,GG1DM,GG2DM) VALUES('{0}','{1}',''+@GG1DM_TMDZB_SHORT+'',''+@GG2DM_TMDZB_SHORT+'')
													END 
													", dataRow["short_url"].ToString(), dataRow["item_id"].ToString(), dataRow["color"].ToString(), dataRow["size"].ToString());

								stringBuilder.Append(str + "\n");
								//插入客户代码
								str = string.Format(@"IF NOT EXISTS(SELECT 1 FROM kehu WHERE khdm='{0}')
														BEGIN   
														INSERT INTO KEHU(KHDM,KHMC,LBDM,QDDM,QYDM,YGDM,BYZD2,JGSD,TJSD,ZK,CKDM,XZDM,TZSY,BYZD25) 
														VALUES('{0}','{0}','000','000','000','000','1','BZSJ','BZSJ',1,'{0}','2','0',getdate())  END   ",
												dataRow["store_id"].ToString());
								stringBuilder.Append(str + "\n");
								// 插入仓库代码
								str = string.Format(@"IF NOT EXISTS(SELECT 1 FROM dbo.CANGKU WHERE CKDM='{0}')  
                                                        BEGIN    
														INSERT INTO CANGKU(CKDM,CKMC,QDDM,YGDM,LBDM,QYDM,XZDM,DH2,JGSD,ZK,TJSD,TZSY)  
														VALUES('{0}','{0}','000','000','000','000','1','1','BZSJ',1,'BZSJ','0')  END    ",
													dataRow["store_id"].ToString());
								stringBuilder.Append(str + "\n");
								// 插入仓库库位
								str = string.Format(@"
                                                    IF NOT EXISTS(SELECT 1 FROM dbo.CKKW WHERE CKDM='{0}')
                                                     BEGIN
                                                    insert into CKKW(CKDM,KWDM,INUSE,INZK,OUTUSE,OUTZK,BYZD2) values('{0}','000','0',1,'0','1',1) 
                                                     END ",
													dataRow["store_id"].ToString());
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
														END  END    ", dataRow["shop_name"].ToString());
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
								// dataRow["store_id"].ToString(), dataRow["item_id"].ToString(), dataRow["color"].ToString(), dataRow["size"].ToString(), dataRow["current_amount"].ToString());
								//stringBuilder.Append(str + "\n");
								if (stringBuilder.ToString() != "")
								{
									try
									{
										testDbManager.ExecuteNonQuery(stringBuilder.ToString());
									}
									catch (Exception ex)
									{
										LogUtil.WriteError(this, "插入商品基础档单 : sql", "sql : " + stringBuilder.ToString() + "错误日志 :" + ex.Message);
									}
								}
								#region //精确查询商品存库信息
								RetailIfashionSkuinfoGetRequest retailIfashionSkuinfoGetRequest = new RetailIfashionSkuinfoGetRequest();
								retailIfashionSkuinfoGetRequest.SkuId = dataRow["sku_id"].ToString();
								retailIfashionSkuinfoGetRequest.ItemId = dataRow["item_id"].ToString();
								try
								{
									RetailIfashionSkuinfoGetResponse retailIfashionSkuinfoGetResponse = topClient.Execute<RetailIfashionSkuinfoGetResponse>(retailIfashionSkuinfoGetRequest, session);
									if (retailIfashionSkuinfoGetResponse != null && retailIfashionSkuinfoGetResponse.Result.SuccessAndHasValue && retailIfashionSkuinfoGetResponse.Result.Data != null)
									{
										LogUtil.WriteInfo(this, "分页查询商店基础信息-商品库存信息 : Body 成功记录", retailIfashionSkuinfoGetResponse.Body);
										RetailIfashionSkuinfoGetResponse.SkuInfoDomain data = retailIfashionSkuinfoGetResponse.Result.Data;
										DataTable dataTable4 = JsonHelper.SetDataTableFromQT<RetailIfashionSkuinfoGetResponse.SkuInfoDomain>(data, "storeskulist");
										//插入到库存调整单
										foreach (DataRow dataRow3 in dataTable4.Rows)
										{
											if (dataRow3["item_id"] != null && dataRow3["item_id"].ToString() != "" && dataRow3["color"] != null && dataRow3["color"].ToString() != "" && dataRow3["size"] != null & dataRow3["size"].ToString() != "")
											{
												#region //新增库存调整单
												if (dataRow3["current_amount"].ToString() != "0")
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
																shopinfo.DM2 = dataRow3["store_id"].ToString();
																shopinfo.SHR = "QT";
																//调整类型
																shopinfo.DM1 = "999";
																shopinfo.RQ = DateTime.Now.ToShortDateString();
																shopinfo.YDJH = dataRow3["store_id"].ToString() + dataRow3["item_id"].ToString() + dataRow3["sku_id"].ToString();
																shopinfo.BZ = "蜻蜓平台对接-库存初始化";
																shopinfo.JE = Math.Ceiling(Convert.ToDouble(dataRow3["item_price"]) / 100 * Convert.ToDouble(dataRow3["current_amount"])).ToString();
																shopinfo.SL = dataRow3["current_amount"].ToString();
																shopinfo.ZDR = "QT";
																dic = DataTableBusiness.SetBusinessDataTable<Regulation>(shopinfo, TableName, "Regulation", TableName, out DJBH);
																dicMX = DataTableBusiness.SetEntryOrderDetail_QT_2(DJBH, TableName, dataRow3, dataRow3["store_id"].ToString());
																YanShouInfo infoYS = new YanShouInfo();
																try
																{
																	infoYS = InvoicesManage.GetYsInfo(DJBH, TableName, "P_API_Oper_CKTZD_SH", "QT");
																}
																catch (Exception ex)
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
																	string sql = string.Format("UPDATE " + TableName + " SET JE=(SELECT SUM(JE) FROM " + TableName + "MX WHERE DJBH='{0}')" +
																	",SL=(SELECT SUM(SL) FROM  " + TableName + "MX WHERE DJBH='{0}')WHERE DJBH='{0}'", DJBH);
																	testDbManager.ExecuteNonQuery(sql);
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
										}
									}
									else
									{
										LogUtil.WriteError(this, "分页查询商店基础信息-商品库存信息：报错 ", "RetailIfashionSkuinfoGetResponse - Body :  " + retailIfashionSkuinfoGetResponse.Body);
									}
								}
								catch (Exception ex)
								{
									LogUtil.WriteError(this, "分页查询商店基础信息-商品库存信息：报错 ", "RetailIfashionSkuinfoGetResponse - Body :  " + ex.Message);
								}
								#endregion 
							}
							DataRow dataRow2 = dataTable3.NewRow();
							dataRow2.BeginEdit();
							foreach (DataColumn dataColumn in dataTable3.Columns)
							{
								if (dataColumn.ColumnName.ToString() != "ID")
								{
									dataRow2[dataColumn.ColumnName] = dataRow[dataColumn.ColumnName];
								}
							}
							dataRow2.EndEdit();
							dataTable3.Rows.Add(dataRow2);
						}
						try
						{
							if (dataTable3.Rows.Count > 0 && testDbManager.SqlBulkCopy(dataTable3, "storeskulist"))
							{
								text = string.Format(@"insert into storesku(storeid,taobao_user_nick,previewid,nextid) values('{0}','{1}','{2}','{3}')",
									skuInfoList[0].StoreId, current_Token.taobao_user_nick.ToString(), text2, text3);
								testDbManager.ExecuteNonQuery(text);
							}
						}
						catch (Exception ex)
						{
							LogUtil.WriteError(this, "error:" + ex.Message);
						}
					}
					else
					{
						LogUtil.WriteError(this, "SetSearchOnlineGoodsInfo : Body", "SetSearchOnlineGoodsInfo - Body :  " + retailIfashionSkuinfoListResponse.Body);
					}
				}
				catch (Exception ex)
				{
					LogUtil.WriteError(this, "SetSearchOnlineGoodsInfo error:" + ex.Message);
				}
			}
		}
		public string BillType
		{
			get
			{
				return "分页查询门店sku列表";
			}
		}

		public string Description
		{
			get
			{
				return "分⻚查询⻔店sku列表";
			}
		}
	}
}
