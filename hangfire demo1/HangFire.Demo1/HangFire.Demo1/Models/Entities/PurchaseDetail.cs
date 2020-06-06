using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HangFire.Demo1.Models.Entities
{
    public class PurchaseDetail
    {
        /// <summary>
        /// 单据编号
        /// </summary>
        public string DJBH { get; set; }
        /// <summary>
        /// 商品代码
        /// </summary>
        public string SPDM { get; set; }

        /// <summary>
        /// 颜色代码
        /// </summary>
        public string GG1DM { get; set; }

        public int MIBH { get; set; }

        /// <summary>
        /// 尺码代码
        /// </summary>
        public string GG2DM { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public string SL { get; set; }
        /// <summary>
        /// 标准价
        /// </summary>
        public string CKJ { get; set; }

        /// <summary>
        /// 折扣
        /// </summary>
        public string ZK { get; set; }
        /// <summary>
        /// 单价
        /// </summary>
        public string DJ { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public string JE { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string BZ { get; set; }
        /// <summary>
        /// 单据号
        /// </summary>
        public string DJH { get; set; }
        /// <summary>
        ///  参考价
        /// </summary>
        public string BYZD12 { get; set; }
        /// <summary>
        /// 标准金额
        /// </summary>
        public string BZJE { get; set; }
        /// <summary>
        /// 行号
        /// </summary>
        public string HH { get; set; }

        public string SL_2 { get; set; }
        public string byzd1 { get; set; }
        /// <summary>
        /// 结算额
        /// </summary>
        public string byzd9 { get; set; }
        /// <summary>
        /// 扣率
        /// </summary>
        public string byzd8 { get; set; }
        public string bz { get; set; }
        public string DJ_1 { get; set; }
        public string DJ_2 { get; set; }
        public int MXBH { get; set; }
        public string byzd4 { get; set; }
        /// <summary>
        /// 批次代码
        /// </summary>
        public string PCDM { get; set; }
    }
}
