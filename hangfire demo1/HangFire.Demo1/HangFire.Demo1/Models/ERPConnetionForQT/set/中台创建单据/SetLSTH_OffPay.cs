using BSERP.Connectors.Efast.Interface;
using BSERP.Connectors.Efast.Log;
using BSERP.Connectors.Efast.Services;
using BSERP.Connectors.Efast.Util;
using ERPApiService.Common.Business;
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
    /// 零售退货单 上传到taostyle 
    /// isonline 线下的单据
    /// 创建单据   taobao.retail.ifashion.order.create
    /// </summary>
    public class SetLSTH_OffPay : StatusTask<QT_SPKCB>, IBillType
    {
		public SetLSTH_OffPay()
		{
			base.Parameter["method"] = "taobao.retail.ifashion.order.create";
		} 
		public override void Run()
		{
			string arg = string.Empty;
			if (!string.IsNullOrWhiteSpace(Config.pullbilldatetime))
			{
				arg = string.Format(" AND CONVERT(BIGINT,LastChanged) > {0} ", Config.pullbilldatetime);
			}
			//没有同步过的单据 is_move -> 0  已经同步过的单据 is_move  -> 1 已经上传库存变更 is_move -> 2
			string sql = string.Format(@"select vMBillID as DJBH,vShop as DM1,fMoney as JE ,CONVERT(bigint,LastChanged) as lastchanged  from SG_Gathering
                        where isnull(SG_Gathering.is_move,'0')='0' and SG_Gathering.fQuantity < 0
                        and isnull(SG_Gathering.isonline,'0')='0' {0} order by lastchanged asc ", arg);

			DataTable dataTable = BusinessDbUtil.GetDataTable(sql);
			if (dataTable != null && dataTable.Rows.Count > 0)
			{
				int count = dataTable.Rows.Count;
				int num = (count + 100 - 1) / 100;
				int num2 = 100;
				int num3 = 0;
				Thread[] array = new Thread[num];
				List<long> LastChanged = new List<long>();
				LastChanged.Add(0L);
				for (int i = 0; i < count; i += 100)
				{
					DataRow[] drs = dataTable.AsEnumerable().Take(num2).Skip(i).ToArray<DataRow>();
					array[num3] = new Thread(()=>
					{
						this.sendLSTH_OffPay(drs, LastChanged);
					});
					array[num3].Name = "线程" + i.ToString();
					array[num3].Start();
					num3++;
					num2 += 100;
				}
			}
		}

		public void sendLSTH_OffPay(DataRow[] drs, List<long> list)
		{
			string app_key = ConfigUtil.App_key;
			string app_secret = ConfigUtil.App_secret;
			string iposApiUrl = ConfigUtil.IposApiUrl;
			ITopClient topClient = new DefaultTopClient(iposApiUrl, app_key, app_secret);
			RetailIfashionOrderCreateRequest retailIfashionOrderCreateRequest = new RetailIfashionOrderCreateRequest();
			for (int i = 0; i < drs.Length; i++)
			{
				DataRow dataRow = drs[i];
				string accessToken_QT = InvoicesManage.GetAccessToken_QT(dataRow["DM1"].ToString());
				RetailIfashionOrderCreateRequest.OrderInfoRequestDomain orderInfoRequestDomain = new RetailIfashionOrderCreateRequest.OrderInfoRequestDomain();
				
				DataTable billInfoByIwms_ForQT = InvoicesManage.GetBillInfoByIwms_XP_ForQT(dataRow["djbh"].ToString());
				string skuList = "";
				if (billInfoByIwms_ForQT != null && billInfoByIwms_ForQT.Rows.Count > 0)
				{
					skuList = JsonParser.ToJson(billInfoByIwms_ForQT);
				}
				orderInfoRequestDomain.SkuList = skuList;
				orderInfoRequestDomain.OrderId = dataRow["DJBH"].ToString();
				orderInfoRequestDomain.TotalFee = Math.Abs(Math.Ceiling(decimal.Parse(dataRow["JE"].ToString()))).ToString(); 
				orderInfoRequestDomain.Source = "baison";
				orderInfoRequestDomain.Type = "erp-sale-back";
				retailIfashionOrderCreateRequest.Param_ = orderInfoRequestDomain;
				try
				{
					RetailIfashionOrderCreateResponse retailIfashionOrderCreateResponse = topClient.Execute<RetailIfashionOrderCreateResponse>(retailIfashionOrderCreateRequest, accessToken_QT);
					if (retailIfashionOrderCreateResponse != null && retailIfashionOrderCreateResponse.Result != null && retailIfashionOrderCreateResponse.Result.Success)
					{
						string sql = "update SG_Gathering set is_move='1' where vMBillID='" + dataRow["DJBH"].ToString() + "'";
						BusinessDbUtil.ExecuteNonQuery(sql);
						LogUtil.WriteInfo(this, retailIfashionOrderCreateResponse.Body, "中台小票上传成功：DJBH" + dataRow["DJBH"].ToString());
						if (Convert.ToInt64(dataRow["LastChanged"]) > list[0])
						{
							this.UpdateStatusPostDate(dataRow["LastChanged"].ToString());
							list[0] = Convert.ToInt64(dataRow["LastChanged"]);
						}
					}
					else
					{
						LogUtil.WriteInfo(this, retailIfashionOrderCreateResponse.Body, string.Format("返回的状态：flag:{0},code:{1},message:{2}", "失败", "失败", retailIfashionOrderCreateResponse.Body));
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
				return "中台单据创建-小票退货单上传";
			}
		}

		public override string Description
		{
			get
			{
				return "SetLSTH_OffPay";
			}
		}
	}
}
