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
using System.Text;
using System.Threading;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;

namespace ERPApiService.Business.ERPConnetionForQT.set
{
    /// <summary>
    /// 商品退货单 采购退货 上传到taostyle  线下的单据is_online = '0'
    /// 创建单据   taobao.retail.ifashion.order.create
    /// </summary>
    public class SetSPTHD_FromERP : StatusTask<QT_GoodsInfo>, IBillType
    {

		public override void Run()
		{
			string _LastChanged = string.Empty;
			if (!string.IsNullOrWhiteSpace(Config.pullbilldatetime))
			{
				_LastChanged = string.Format(" AND CONVERT(BIGINT,t0.LastChanged) > {0} ", Config.pullbilldatetime);
			}
			string sql = string.Format(@"select t0.BYZD3,t0.qddm,T0.DJBH,'SPTHD' AS TableName,t0.SL,T0.JE,t0.DM1,
                                        T0.DM4,t0.DM2 AS ckCode,t0.RQ AS created ,t0.zdr,t0.YXRQ,t0.BZ,
                                        CONVERT(BIGINT,t0.LastChanged) as LastChanged from 
                                        SPTHD t0  INNER JOIN CANGKU ON CANGKU.CKDM=t0.DM2  
                                        where  isnull(T0.Is_Move,0)='0' and isnull(T0.isonline,'0')='0' 
                                        and isnull(t0.SH,0)=1 and t0.SL>0  {0} order by t0.LastChanged desc", _LastChanged);
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
						this.sendSPTHD_FromERP(drs, LastChanged);
					});
					array[num3].Name = "线程" + i.ToString();
					array[num3].Start();
					num3++;
					num2 += 100;
				}
			}
		}
		public void sendSPTHD_FromERP(DataRow[] drs, List<long> list)
		{
			string app_key = ConfigUtil.App_key;
			string app_secret = ConfigUtil.App_secret;
			string iposApiUrl = ConfigUtil.IposApiUrl;
			ITopClient topClient = new DefaultTopClient(iposApiUrl, app_key, app_secret);
			RetailIfashionOrderCreateRequest retailIfashionOrderCreateRequest = new RetailIfashionOrderCreateRequest();
			for (int i = 0; i < drs.Length; i++)
			{
				#region
				DataRow dataRow = drs[i];
				try
				{
					string accessToken_QT = InvoicesManage.GetAccessToken_QT(dataRow["DM1"].ToString(), "SDTHD");
					RetailIfashionOrderCreateRequest.OrderInfoRequestDomain orderInfoRequestDomain = new RetailIfashionOrderCreateRequest.OrderInfoRequestDomain();
					DataTable billInfoByIwms_ForQT = InvoicesManage.GetBillInfoByIwms_ForQT("SPTHDMX", dataRow["djbh"].ToString());
					DataRow drList = billInfoByIwms_ForQT.AsEnumerable().Where(item => item["sku_id"].ToString() == "").FirstOrDefault();
					if (drList != null)
					{
						LogUtil.WriteError(this, "条码不存在", JsonParser.ToJson(billInfoByIwms_ForQT));
						continue;
					}
					string skuList = "";
					if (billInfoByIwms_ForQT != null && billInfoByIwms_ForQT.Rows.Count > 0)
					{
						skuList = JsonParser.ToJson(billInfoByIwms_ForQT);
					}

					orderInfoRequestDomain.SkuList = skuList;
					orderInfoRequestDomain.OrderId = dataRow["DJBH"].ToString();
					orderInfoRequestDomain.TotalFee = Math.Ceiling(decimal.Parse(dataRow["JE"].ToString()) * 100).ToString();
					orderInfoRequestDomain.Source = "baison";
					orderInfoRequestDomain.Type = "erp-stock-back";
					retailIfashionOrderCreateRequest.Param_ = orderInfoRequestDomain;
					try
					{
						RetailIfashionOrderCreateResponse retailIfashionOrderCreateResponse = topClient.Execute<RetailIfashionOrderCreateResponse>(retailIfashionOrderCreateRequest, accessToken_QT);
						if (retailIfashionOrderCreateResponse != null && retailIfashionOrderCreateResponse.Result != null && retailIfashionOrderCreateResponse.Result.Success)
						{
							string sql = "update SPTHD set is_move='1' where djbh='" + dataRow["DJBH"].ToString() + "'";
							BusinessDbUtil.ExecuteNonQuery(sql);
							LogUtil.WriteInfo(this, retailIfashionOrderCreateResponse.Body, "中台商品退货单单据上传成功：DJBH"+ dataRow["DJBH"].ToString());
							if (Convert.ToInt64(dataRow["LastChanged"]) > list[0])
							{
								this.UpdateStatusPostDate(dataRow["LastChanged"].ToString());
								list[0] = Convert.ToInt64(dataRow["LastChanged"]);
							}
						}
						else
						{
							LogUtil.WriteInfo(this, retailIfashionOrderCreateResponse.Body, string.Format("返回的状态：flag:{0},code:{1},message:{2}", retailIfashionOrderCreateResponse.Body, retailIfashionOrderCreateResponse.Body, retailIfashionOrderCreateResponse.Body));
						}
					}
					catch (Exception ex)
					{
						LogUtil.WriteError(this, ex.Message, ex.Message);
					}
					#endregion
				}
				catch (System.Exception ex) 
				{
					LogUtil.WriteError(this, "其他错误", ex.Message);
				}
			}
        }
		public override string BillType
		{
			get
			{
				return "中台单据创建-商品退货单上传";
			}
		} 
		public override string Description
		{
			get
			{
				return "SetSPTHD_FromERP";
			}
		}
	}
}
