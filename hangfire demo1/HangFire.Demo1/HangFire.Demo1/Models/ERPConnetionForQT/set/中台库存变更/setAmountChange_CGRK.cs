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
    /// 采购入库 - 库存变更
    /// </summary>
    public class setAmountChange_CGRK : StatusTask<QT_GoodsInfo>, IBillType
    {
		public override void Run()
		{
			string _LastChanged = string.Empty;
			if (!string.IsNullOrWhiteSpace(Config.pullbilldatetime))
			{
				_LastChanged = string.Format(" AND CONVERT(BIGINT,t0.LastChanged) > {0} ", Config.pullbilldatetime);
			}
			string sql = string.Format(@"select t0.BYZD3,t0.qddm,T0.DJBH,'SPJHD' AS TableName,t0.SL,T0.JE,t0.DM1,
                                        T0.DM4,t0.DM2 AS ckCode,t0.RQ AS created ,t0.zdr,t0.YXRQ,t0.BZ from 
                                        SPJHD t0  INNER JOIN CANGKU ON CANGKU.CKDM=t0.DM2  
                                        where  isnull(T0.Is_Move,0)='0' and isnull(T0.isonline,'0')='0' 
                                        and isnull(t0.SH,0)=1 and t0.SL>0  {0}", _LastChanged);
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
					array[num3] = new Thread(() =>
					{
						this.sendSPJHD_FromERP(drs, LastChanged);
					});
					array[num3].Name = "线程" + i.ToString();
					array[num3].Start();
					num3++;
					num2 += 100;
				}
			}
		}
		public void sendSPJHD_FromERP(DataRow[] drs, List<long> list)
		{
			string app_key = ConfigUtil.App_key;
			string app_secret = ConfigUtil.App_secret;
			string iposApiUrl = ConfigUtil.IposApiUrl;
			ITopClient topClient = new DefaultTopClient(iposApiUrl, app_key, app_secret);
			RetailIfashionItemamountChangeRequest req = new RetailIfashionItemamountChangeRequest();
			for (int i = 0; i < drs.Length; i++)
			{
				DataRow dataRow = drs[i];
				string accessToken_QT = InvoicesManage.GetAccessToken_QT(dataRow["DM1"].ToString(), "SPJHD");

				RetailIfashionItemamountChangeRequest.SkuInfoRequestDomain obj1 = new RetailIfashionItemamountChangeRequest.SkuInfoRequestDomain();
				DataTable billInfoByIwms_ForQT = InvoicesManage.GetBillInfoByIwms_setAmountChange_ForQT("SPJHD", dataRow["DJBH"].ToString());
				if (billInfoByIwms_ForQT != null && billInfoByIwms_ForQT.Rows.Count > 0)
				{
					bool flag = true;
					foreach (DataRow dr in billInfoByIwms_ForQT.Rows)
					{
						obj1.Source = "baison";
						obj1.ItemId = dr["SPDM"].ToString();
						obj1.SkuId = dr["SPTM"].ToString();
						obj1.Type = "erp-stock-in";
						obj1.Amount = Convert.ToInt32(dr["SL"]).ToString();
						obj1.Datetime = dr["LastChanged"].ToString();
						req.Param_ = obj1;
						try
						{
							RetailIfashionItemamountChangeResponse retailIfashionOrderCreateResponse = topClient.Execute<RetailIfashionItemamountChangeResponse>(req, accessToken_QT);
							if (retailIfashionOrderCreateResponse != null && retailIfashionOrderCreateResponse.Result != null && retailIfashionOrderCreateResponse.Result.Success)
							{
								//所有的明细都上传完成
								flag = flag && true;
							}
							else
							{
								//存在有明细上传失败
								flag = flag && false;
								//如果失败了更新
								//上传失败 原因是因为明细不符合
								string sql = "update SPJHD set is_move='3',BZ='上传蜻蜓失败，蜻蜓的商品条码明细不存在' where djbh='" + dataRow["DJBH"].ToString() + "'";
								BusinessDbUtil.ExecuteNonQuery(sql);
								LogUtil.WriteError(this, "明细商品中没有对应的条码明细", "SPJHD 对应的单号 ：" + dataRow["DJBH"].ToString());
								//LogUtil.WriteInfo(this, retailIfashionOrderCreateResponse.Body, string.Format("返回的状态：flag:{0},code:{1},message:{2}", "成功", "成功", retailIfashionOrderCreateResponse.Body));
							}
						}
						catch (Exception ex)
						{
							LogUtil.WriteError(this, ex.Message, ex.Message);
						}
					}
					if (flag)
					{
						//库存变更接口 上传成功
						string sql = "update SPJHD set is_move='2' where djbh='" + dataRow["DJBH"].ToString() + "'";
						BusinessDbUtil.ExecuteNonQuery(sql);
						LogUtil.WriteInfo(this, "上传蜻蜓平台的采购进货单成功", "SPJHD 对应的单号 ：" + dataRow["DJBH"].ToString());
					}
				}
				else
				{
					//上传失败 原因是因为明细不符合
					string sql = "update SPJHD set is_move='3',BZ='上传蜻蜓失败，明细条码不存在' where djbh='" + dataRow["DJBH"].ToString() + "'";
					BusinessDbUtil.ExecuteNonQuery(sql);
					LogUtil.WriteError(this, "明细商品中没有对应的条码明细", "SPJHD 对应的单号 ：" + dataRow["DJBH"].ToString());
				}
			}
		}
		public override string BillType
		{
			get
			{
				return "中台库存变更-采购入库";
			}
		}
		public override string Description
		{
			get
			{
				return "setAmountChange_CGRK";
			}
		}
	}
}
