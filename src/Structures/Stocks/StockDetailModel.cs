using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structures.Stocks
{
    public class StockDetailModel : StockBase
    {
        public double Price { get; set; } = 0.001;

        public double ChangedPrice { get; set; } = 0.001;

        public double ChangedPercent { get; set; } = 0.001;

        /// <summary>
        /// 总手,单位(手)
        /// </summary>
        public double TodayTotalHand { get; set; } = 0.001;

        /// <summary>
        /// 昨日收盘
        /// </summary>
        public double YesterdayEndPrice { get; set; } = 0.001;

        /// <summary>
        /// 金额
        /// </summary>
        public double TodayAmount { get; set; } = 0.001;

        /// <summary>
        /// 总值
        /// </summary>
        public double TotalValue { get; set; } = 0.001;

        /// <summary>
        /// 流值
        /// </summary>
        public double FlowlValue { get; set; } = 0.001;
    }
}
