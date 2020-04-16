using System.Collections.Generic;

namespace HangFire.Demo1.Models.ERPConnetionForQT.entity
{
	public class QT_LSXHD_OffPay
	{
		public List<QT_LSXHD_OffPayMX> skuList = new List<QT_LSXHD_OffPayMX>();

		public string orderId
		{
			get;
			set;
		}

		public decimal totalFee
		{
			get;
			set;
		}

		public string type
		{
			get;
			set;
		}
	}
}
