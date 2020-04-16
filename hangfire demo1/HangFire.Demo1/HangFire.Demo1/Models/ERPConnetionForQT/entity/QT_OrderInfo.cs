using System.Collections.Generic;

namespace HangFire.Demo1.Models.ERPConnetionForQT.entity
{
	public class QT_OrderInfo
	{
		public List<skuList> skuLists = new List<skuList>();

		public string orderId
		{
			get;
			set;
		}

		public string storeId
		{
			get;
			set;
		}

		public string itemId
		{
			get;
			set;
		}

		public string skuId
		{
			get;
			set;
		}

		public string currentAmount
		{
			get;
			set;
		}

		public string amount
		{
			get;
			set;
		}

		public string totalFee
		{
			get;
			set;
		}

		public string method
		{
			get;
			set;
		}
	}
}
