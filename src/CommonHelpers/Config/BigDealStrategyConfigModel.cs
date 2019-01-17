using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelpers.Config
{
    public class BigDealStrategyConfigModel
    {
        /// <summary>
        /// 每个分组的股票容量
        /// </summary>
        [JsonProperty("ClawlerConfigData")]        
        public int StockMaxCountEachGroup { get; set; }

        /// <summary>
        /// 大单策略使用分析数据的线程数量
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
        /// 大单的界限
        /// </summary>
        [JsonProperty("BigDealAmountThreshold")]
        public double BigDealAmountThreshold { get; set; }        
        
        /// <summary>
        /// 在范围内大单的个数界限
        /// </summary>
        [JsonProperty("BigDealCountThreshold")]
        public double BigDealCountThreshold { get; set; }
    }
}
