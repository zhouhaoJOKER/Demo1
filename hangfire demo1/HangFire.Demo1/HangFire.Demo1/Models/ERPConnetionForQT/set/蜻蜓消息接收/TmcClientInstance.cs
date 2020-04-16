using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using BSERP.Connectors.Efast.Interface;
using BSERP.Connectors.Efast.Log;
using BSERP.Connectors.Efast.Util;
using ERPApiService.Business.EntityTable;
using ERPApiService.Business.ERPConnetionE3.entity;
using ERPApiService.Business.ERPConnetionIWMS.entity;
using ERPApiService.Common.Business;
using ERPApiService.Common.helper;
using ERPApiService.Common.Object;
using HangFire.Demo1.Models.ERPConnetionForQT.entity;
using Top.Tmc;

namespace ERPApiService.Business.ERPConnetionForQT.entity
{
    /// <summary>
    /// taostyle 消息通知  
    /// </summary>
    public class TmcClientInstance : IBillType
	{
		private static object lockobject = new object();

		public string BillType
		{
			get
			{
				return "消息通知创建";
			}
		}

		public string Description
		{
			get
			{
				return "消息通知创建";
			}
		}

		public void clientCallBack()
		{
			string app_key = ConfigUtil.App_key;
			string app_secret = ConfigUtil.App_secret;
			TmcClient tmcClient = new TmcClient(app_key, app_secret, "default");
			LogUtil.WriteInfo(this, "clientCallBack : 消息监听中", "appkey:" + app_key + " ;appsecret : " + app_secret);
			tmcClient.OnMessage += delegate (object s, MessageArgs e)
			{
				try
				{
					DbOperation dbOperation = new DbOperation(ConfigUtil.ConnectionString);
					LogUtil.WriteInfo(this, "e.Message.Topic:", e.Message.Topic);
					LogUtil.WriteInfo(this, "e.Message.Content:", e.Message.Content);
					//消息通知-商品基础消息创建  "taobao_ifashion_ItemInfoCreate"
					if (e.Message.Topic == "taobao_ifashion_ItemInfoCreate")
					{
						try
						{
							QT_GoodsInfo qT_GoodsInfo = JsonParser.FromJson<QT_GoodsInfo>(e.Message.Content);
							string text = string.Format(@"if not exists (select 1 from storesku_mid WITH(NOLOCK)
                                where store_id='{0}' and item_id='{1}' and sku_id='{2}' )
								begin
								insert into storesku_mid(store_id,item_id,sku_id) values('{0}','{1}','{2}') 
								end",  qT_GoodsInfo.store_id, qT_GoodsInfo.item_id, qT_GoodsInfo.sku_id
							);
							dbOperation.ExecuteNonQuery(text);
						}
						catch (Exception ex)
						{
							LogUtil.WriteError(this, "e.Message.Topic: taobao_ifashion_ItemInfoCreate ", "入参数据传入错误，请检查数据是否正确." + ex.Message);
						}
					}
					//消息通知-库存变更 "taobao_ifashion_ItemAmountChanged" 
					//{"amount":2,"item_id":604769130652,"msg_id":"300101a2c3f8445dade85057db0a31fc","current_amount":2,"store_id":947,"sku_id":4407918687796,"order_id":1064001,"type":"qt-stock-in"}
					else if (e.Message.Topic == "taobao_ifashion_ItemAmountChanged")
					{
						try
						{
							lock (TmcClientInstance.lockobject)
							{
								QT_GoodsInfo qT_GoodsInfo = JsonParser.FromJson<QT_GoodsInfo>(e.Message.Content);

								string text = string.Format(@"if not exists (select 1 from ItemAmountChanged WITH(NOLOCK)
                                where item_id='{0}' and store_id='{4}' and sku_id='{5}' and order_id='{6}')
								begin
								 insert into ItemAmountChanged(amount,current_amount,item_id,store_id,sku_id,order_id,order_type) 
                                 values('{1}','{2}','{3}','{4}','{5}','{6}','{7}') 
								end", qT_GoodsInfo.item_id,qT_GoodsInfo.amount, qT_GoodsInfo.current_amount, qT_GoodsInfo.item_id, qT_GoodsInfo.store_id,
									qT_GoodsInfo.sku_id, qT_GoodsInfo.order_id, qT_GoodsInfo.type
								);
								dbOperation.ExecuteNonQuery(text);
							}
						}
						catch (Exception ex)
						{
							LogUtil.WriteError(this, "e.Message.Topic: taobao_ifashion_ItemAmountChanged ", "入参数据传入错误，请检查数据是否正确." + ex.Message);
						}
					}
					//消息通知-创建单据 "taobao_ifashion_OrderCreate"
					else if (e.Message.Topic == "taobao_ifashion_OrderCreate")
					{
						try
						{
							lock (TmcClientInstance.lockobject)
							{
								QT_GoodsInfo qT_GoodsInfo2 = JsonParser.FromJson<QT_GoodsInfo>(e.Message.Content);
								string text = string.Format(@"if not exists (select 1 from
                                        OrderCreate WITH(NOLOCK) where storeId='{0}' and orderId='{1}' )  
										begin
											insert into OrderCreate(storeId,orderId,orderType) values('{0}','{1}','{2}')
                                        end",  qT_GoodsInfo2.store_id, qT_GoodsInfo2.order_id, qT_GoodsInfo2.type
								 );
								dbOperation.ExecuteNonQuery(text);
							}
						}
						catch (Exception ex)
						{
							LogUtil.WriteError(this, "e.Message.Topic: taobao_ifashion_OrderCreate ", "入参数据传入错误，请检查数据是否正确." + ex.Message + " \n e.Message.Content :  " + e.Message.Content);
						}
					}
				}
				catch (Exception ex2)
				{
					LogUtil.WriteError(this, "获取消息失败：message:"+ex2.Message);
					Console.WriteLine(ex2.StackTrace);
					e.Fail();
				}
			};             
			tmcClient.Connect("ws://mc.api.taobao.com/");
			Thread.Sleep(2000);
		}
	}
}
