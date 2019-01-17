using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelpers.Config
{
    public class QuickUpStrategyConfigModel
    {
        /// <summary>
        /// 每个分组的股票容量
        /// </summary>
        [JsonProperty("ClawlerConfigData")]
        public int StockMaxCountEachGroup { get; set; }

        /// <summary>
        /// 快速上涨策略使用分析数据的线程数量
        /// </summary>
        [JsonProperty("ThreadCount")]
        public int ThreadCount { get; set; }

        /// <summary>
        /// 分析某一笔时,向前 截取的时间范围
        /// </summary>
        [JsonProperty("ForwardSeconds")]
        public int ForwardSeconds { get; set; }

        /// <summary>
        /// 分析某一笔时,向后 截取的时间范围
        /// </summary>
        [JsonProperty("AfterSeconds")]
        public int AfterSeconds { get; set; }

        /// <summary>
        /// 筛选数据时的成交金额的最低界限
        /// </summary>
        [JsonProperty("DealAmountThreshold")]
        public double DealAmountThreshold { get; set; }

        /// <summary>
        /// 快速上涨幅度的界限
        /// </summary>
        [JsonProperty("QuickUpThreshold")]
        public double QuickUpThreshold { get; set; }
    }
}
