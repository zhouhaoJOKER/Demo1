using System.Text;

namespace HangFire.Demo1.Models.ERPConnetionForQT.entity
{
	public class QT_SPKCB
	{
		public string storeld
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

		public string ToXMLString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
			stringBuilder.Append("<request>");
			stringBuilder.Append("<orderinfo>");
			stringBuilder.Append("<storeld>" + storeld + "</storeld>");
			stringBuilder.Append("<itemId>" + itemId + "</itemId>");
			stringBuilder.Append("<skuId>" + skuId + "</skuId>");
			stringBuilder.Append("<currentAmount>" + currentAmount + "</currentAmount>");
			stringBuilder.Append("</orderinfo>");
			stringBuilder.Append("</request>");
			return stringBuilder.ToString();
		}
	}
}
