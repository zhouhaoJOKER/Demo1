using BSERP.Connectors.Efast.Interface;
using BSERP.Connectors.Efast.Log;
using BSERP.Connectors.Efast.Services;
using BSERP.Connectors.Efast.Util;
using ERPApiService.Common.Business;
using HangFire.Demo1.Models.ERPConnetionForQT.entity;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;

namespace ERPApiService.Business.ERPConnetionForQT.set
{
    public class SetSPKCBForQT : StatusTask<QT_SPKCB>, IBillType
    {
		public override void Run()
		{
			this.CreateQT_SPKCB();
		}
		public void CreateQT_SPKCB()
		{
			try
			{
				string arg = string.Empty;
				if (!string.IsNullOrWhiteSpace(base.Config.pullbilldatetime))
				{
					arg = string.Format(" AND CONVERT(BIGINT,spkcb.LastChanged) > {0} ", base.Config.pullbilldatetime);
				}
				else
				{
					arg = " AND 1=1";
				}
				//条码取自条码对照表数据
				//string sql = string.Format(@"select CKDM,spkcb.SPDM,TMDZB.SPTM as SKU,1 as SL,CONVERT(BIGINT,spkcb.LastChanged) as LastChanged from spkcb
				//						inner join TMDZB on spkcb.SPDM+spkcb.GG1DM+spkcb.GG2DM=TMDZB.SPDM+TMDZB.GG1DM+TMDZB.GG2DM
				//						 where CKDM<>'' 
				//						AND CKDM ='947' and KWDM='000' and spkcb.SPDM='602695726815' and spkcb.GG1DM='SZHS' and spkcb.GG2DM='1003'
				//						group by  CKDM,TMDZB.SPTM,spkcb.SPDM,spkcb.GG1DM,spkcb.GG2DM,spkcb.LastChanged
				//						order by LastChanged asc", arg);
				
				string sql = string.Format(@"select CKDM,spkcb.SPDM,TMDZB.SPTM as SKU,sum(spkcb.SL) as SL,CONVERT(BIGINT,spkcb.LastChanged) as LastChanged from spkcb
											left join TMDZB on spkcb.SPDM+spkcb.GG1DM+spkcb.GG2DM = TMDZB.SPDM+TMDZB.GG1DM+TMDZB.GG2DM 
											left join SHANGPIN on TMDZB.SPDM=SHANGPIN.SPDM 
											where  CKDM<>'' and SHANGPIN.BZHU='蜻蜓平台同步'  {0}
											group by  CKDM,TMDZB.SPTM,spkcb.SPDM,spkcb.GG1DM,spkcb.GG2DM,spkcb.LastChanged
											order by spkcb.LastChanged asc", arg);

				DataTable dataTable = BusinessDbUtil.GetDataTable(sql);
				if (dataTable != null && dataTable.Rows.Count > 0)
				{
					int count = dataTable.Rows.Count;
					int num = (count + 100 - 1) / 100;
					int num2 = 100;
					int num3 = 0; 

					Thread[] array = new Thread[num];
					List<long> LastChanged = new List<long>();
					LastChanged.Add(0);
					for (int i = 0; i < count; i += 100)
					{
						DataRow[] drs = dataTable.AsEnumerable().Take(num2).Skip(i).ToArray<DataRow>();
						array[num3] = new Thread(() =>
						{
							this.sendSPKCBInfo(drs, LastChanged);
						});
						array[num3].Name = "线程" + i.ToString();
						array[num3].Start();
						num3++;
						num2 += 100;
					}

				}
			}
			catch (Exception ex)
			{
				LogUtil.WriteError(this, ex.Message, ex.Message);
			}
		}

		public void sendSPKCBInfo(DataRow[] drs, List<long> list)
		{
			string app_key = ConfigUtil.App_key;
			string app_secret = ConfigUtil.App_secret;
			string iposApiUrl = ConfigUtil.IposApiUrl;
			ITopClient topClient = new DefaultTopClient(iposApiUrl, app_key, app_secret);
			RetailIfashionItemamountSyncRequest retailIfashionItemamountSyncRequest = new RetailIfashionItemamountSyncRequest();
			for (int i = 0; i < drs.Length; i++) 
			{
				DataRow dataRow = drs[i];
				string accessToken_QT = InvoicesManage.GetAccessToken_QT(dataRow["CKDM"].ToString());
				RetailIfashionItemamountSyncRequest.SkuInfoRequestDomain skuInfoRequestDomain = new RetailIfashionItemamountSyncRequest.SkuInfoRequestDomain();
				skuInfoRequestDomain.ItemId = dataRow["SPDM"].ToString();
				skuInfoRequestDomain.SkuId = dataRow["SKU"].ToString();
				skuInfoRequestDomain.CurrentAmount = dataRow["SL"].ToString();
				skuInfoRequestDomain.Source = "baison";
				skuInfoRequestDomain.Datetime = dataRow["LastChanged"].ToString();
				retailIfashionItemamountSyncRequest.Param_ = skuInfoRequestDomain;
				try
				{
					LogUtil.WriteInfo(this, "商品库存表同步 ：" + JsonParser.ToJson(retailIfashionItemamountSyncRequest), "请求的数据：" + JsonParser.ToJson(skuInfoRequestDomain));
					RetailIfashionItemamountSyncResponse retailIfashionItemamountSyncResponse = topClient.Execute<RetailIfashionItemamountSyncResponse>(retailIfashionItemamountSyncRequest, accessToken_QT);
					if (retailIfashionItemamountSyncResponse != null && retailIfashionItemamountSyncResponse.Result != null && retailIfashionItemamountSyncResponse.Result.Success)
					{
						if (Convert.ToInt64(dataRow["LastChanged"]) > list[0])
						{
							this.UpdateStatusPostDate(dataRow["LastChanged"].ToString()); 
						}
						LogUtil.WriteInfo(this, retailIfashionItemamountSyncResponse.Body, string.Format("返回的状态：flag:{0},code:{1},message:{2}", retailIfashionItemamountSyncResponse.Body, retailIfashionItemamountSyncResponse.Body, retailIfashionItemamountSyncResponse.Body));
					}
					else
					{
						LogUtil.WriteInfo(this, retailIfashionItemamountSyncResponse.Body, string.Format("返回的状态：flag:{0},code:{1},message:{2}", retailIfashionItemamountSyncResponse.Body, retailIfashionItemamountSyncResponse.Body, retailIfashionItemamountSyncResponse.Body));
					}
				}
				catch (Exception ex)
				{
					LogUtil.WriteError(this, ex.Message, ex.Message);
				}
			}
		}
		public static string GetTimeStamp()
		{
			return Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds).ToString();
		}
		public override string BillType
		{
			get
			{
				return "轻蜓平台商品库存同步(ERP商品库存表)";
			}
		}

		public override string Description
		{
			get
			{
				return "SetSPKCBForQT";
			}
		}
	}
}
