using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HangFire.Demo1.Models.Entities
{
    public class Regulation
    {

        /// <summary>
        /// 订单编号
        /// </summary>
        public string DJBH { get; set; }
        /// <summary>
        /// 原单据号
        /// </summary>
        public string YDJH { get; set; }
        /// <summary>
        /// 订单号
        /// </summary>
        public string LXDJ { get; set; }
        /// <summary>
        /// 调整类型
        /// </summary>
        public string DM1
        {
            get;
            set;
        }
        /// <summary>
        /// 数量
        /// </summary>
        public string SL { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public string JE { get; set; }
        /// <summary>
        /// 制单人
        /// </summary>

        public string ZDR
        {
            get;
            set;
        }
        /// <summary>
        /// 价格选定
        /// </summary>
        public string BYZD5
        {
            get;
            set;
        }
        /// <summary>
        /// 制单日
        /// </summary>
        public DateTime RQ_4 { get; set; }
        /// <summary>
        /// 审核人
        /// </summary>
        public string SHR
        {
            get;
            set;
        }
        /// <summary>
        /// 审核
        /// </summary>
        public string SH { get; set; }
        /// <summary>
        /// 验收
        /// </summary>
        public string YS { get; set; }
        /// <summary>
        /// 验收日期
        /// </summary>
        public string YSRQ { get; set; }
        /// <summary>
        /// 验收人
        /// </summary>
        public string YSR { get; set; }
        /// <summary>
        /// 审核日期
        /// </summary>
        public string SHRQ { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string BZ { get; set; }

        /// <summary>
        /// 商店仓
        /// </summary>
        public string DM2
        {
            get;
            set;
        }
        /// <summary>
        /// 日期
        /// </summary>
        public string RQ { get; set; }
        /// <summary>
        /// 备用
        /// </summary>
        public string XC { get; set; }
        /// <summary>
        /// 订单性质
        /// </summary>
        public string DJXZ { get; set; }
        /// <summary>
        /// 价格选定
        /// </summary>
        public string BYZD1
        {
            get;
            set;
        }
        /// <summary>
        /// 折扣
        /// </summary>
        public string BYZD12
        {
            get;
            set;
        }
        /// <summary>
        /// 渠道代码
        /// </summary>
        public string QDDM { get; set; }
        /// <summary>
        /// 商店库位
        /// </summary>
        public string DM2_1
        {
            get;
            set;
        }
        /// <summary>
        /// 员工代码
        /// </summary>
        public string YGDM { get; set; }


        /// <summary>
        /// 通知单
        /// </summary>
        public string BYZD3 { get; set; }
        /// <summary>
        /// 记账
        /// </summary>
        public string JZ { get; set; }

    }
}
