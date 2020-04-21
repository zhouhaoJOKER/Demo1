using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HangFire.Demo1.Models.Entities
{
    public class YanShouInfo
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// 用户
        /// </summary>
        public string User { get; set; }
        /// <summary>
        /// 单据编号
        /// </summary>
        public string DJBH { get; set; }
        /// <summary>
        /// 存储过程
        /// </summary>
        public string Procedure { get; set; }

        public string DM1 { get; set; }
        public string DM2 { get; set; }
        /// <summary>
        /// 通知单表名
        /// </summary>
        public string SYDJ { get; set; }
        /// <summary>
        /// 通知单编号
        /// </summary>
        public string BYZD3 { get; set; }
        public int type { get; set; }
        public string DBDATE { get; set; }
    }
}
