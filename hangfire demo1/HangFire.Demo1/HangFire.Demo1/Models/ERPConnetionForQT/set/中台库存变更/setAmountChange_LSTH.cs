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
using System.Threading.Tasks;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;

namespace ERPApiService.Business.ERPConnetionForQT.set
{
    public class setAmountChange_LSTH : StatusTask<QT_SPKCB>, IBillType
    {
		/// <summary>
		/// 库存变更 零售退货 taobao.retail.ifashion.itemamount.change
		/// 没有同步过的单据 is_move -> 0  已经同步过的单据 is_move  -> 1 已经上传库存变更 is_move -> 2
		/// </summary> 
		public override void Run()
		{
			string arg = string.Empty;
			if (!string.IsNullOrWhiteSpace(Config.pullbilldatetime))
			{
				arg = string.Format(" AND CONVERT(BIGINT,LastChanged) > {0} ", Config.pullbilldatetime);
			}
			//没有同步过的单据 is_move -> 0  已经同步过的单据 is_move  -> 1 已经上传库存变更 is_move -> 2
			string sql = string.Format(@"select vMBillID as DJBH,vShop as DM1,fMoney as JE ,CONVERT(bigint,LastChanged) as lastchanged  from SG_Gathering
                                        where isnull(SG_Gathering.is_move,'0')='1' and SG_Gathering.fQuantity < 0
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
					array[num3] = new Thread(() =>
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
			//库存变更
			RetailIfashionItemamountChangeRequest req = new RetailIfashionItemamountChangeRequest(); 
			for (int i = 0; i < drs.Length; i++)
			{
				DataRow dataRow = drs[i];
				string accessToken_QT = InvoicesManage.GetAccessToken_QT(dataRow["DM1"].ToString());

				RetailIfashionItemamountChangeRequest.SkuInfoRequestDomain obj1 = new RetailIfashionItemamountChangeRequest.SkuInfoRequestDomain();
				DataTable billInfoByIwms_ForQT = InvoicesManage.GetBillInfoByIwms_setAmountChange_XP_ForQT(dataRow["DJBH"].ToString());
				if (billInfoByIwms_ForQT != null && billInfoByIwms_ForQT.Rows.Count > 0)
				{
					bool flag = true;
					foreach (DataRow dr in billInfoByIwms_ForQT.Rows)
					{
						obj1.Source = "baison";
						obj1.ItemId = dr["SPDM"].ToString();
						obj1.SkuId = dr["SPTM"].ToString();
						obj1.Type = "erp-sale-back";
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
								LogUtil.WriteInfo(this, retailIfashionOrderCreateResponse.Body, string.Format("返回的状态：flag:{0},code:{1},message:{2}", retailIfashionOrderCreateResponse.Body, retailIfashionOrderCreateResponse.Body, retailIfashionOrderCreateResponse.Body));
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
						string sql = "update SG_Gathering set is_move='2' where vMBillID='" + dataRow["DJBH"].ToString() + "'";
						BusinessDbUtil.ExecuteNonQuery(sql);
						LogUtil.WriteInfo(this, "记录上传小票成功：" + dataRow["djbh"].ToString(), "上传到蜻蜓平台成功！");
					}
				}
				else
				{
					//上传失败 原因是因为明细不符合
					string sql = "update SG_Gathering set is_move='3',vMemo='上传蜻蜓失败，明细条码不存在' where vMBillID='" + dataRow["DJBH"].ToString() + "'";
					BusinessDbUtil.ExecuteNonQuery(sql);
					LogUtil.WriteError(this, "明细商品中没有对应的条码明细", "SG_Gathering 对应的单号 ：" + dataRow["djbh"].ToString());
				}
			}
		}

		public override string BillType
		{
			get
			{
				return "中台库存变更-零售退货";
			}
		}
		public override string Description
		{
			get
			{
				return "setAmountChange_LSXH";
			}
		}
	}
}
