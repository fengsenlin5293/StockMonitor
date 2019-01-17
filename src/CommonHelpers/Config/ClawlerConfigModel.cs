using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelpers.Config
{
    public class ClawlerConfigModel
    {
        /// <summary>
        /// 每个线程处理的个股数量
        /// </summary>
        [JsonProperty("CodeCountPerThread")]
        public int CodeCountPerThread { get; set; }  
        
        /// <summary>
        /// 每次请求的数量
        /// </summary>
        [JsonProperty("QueryCountPerTime")]
        public int QueryCountPerTime { get; set; }

        /// <summary>
        /// 每次请求时间间隔(ms)
        /// </summary>
        [JsonProperty("QueryInterval")]
        public int QueryInterval { get; set; }
    }
}
