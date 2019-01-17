using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelpers.Config
{
    public class ConfigJsonModel
    {
        [JsonProperty("IsUseProxy")]
        public bool IsUseProxy { get; set; }

        [JsonProperty("ProxyData")]
        public ProxyModel ProxyData { get; set; }

        [JsonProperty("IsFromConditionFilter")]
        public bool IsFromConditionFilter { get; set; }

        [JsonProperty("FromConditionFilterData")]
        public FromConditionFilterModel FromConditionFilterData { get; set; }

        [JsonProperty("IsFromLocalFile")]
        public bool IsFromLocalFile { get; set; }

        [JsonProperty("MonitorFilePath")]
        public string MonitorFilePath { get; set; }

        [JsonProperty("IsExceptMonitorRange")]
        public bool IsExceptMonitorRange { get; set; }

        [JsonProperty("ExceptMonitorFilePath")]
        public string ExceptMonitorFilePath { get; set; }

        [JsonProperty("IsUseBigOrderStrategy")]
        public bool IsUseBigOrderStrategy { get; set; }

        [JsonProperty("IsUseRapidRiseStrategy")]
        public bool IsUseRapidRiseStrategy { get; set; }

        [JsonProperty("ClawlerConfigData")]
        public ClawlerConfigModel ClawlerConfigData { get; set; }

        [JsonProperty("BigDealStrategyConfigData")]
        public BigDealStrategyConfigModel BigDealStrategyConfigData { get; set; }

        [JsonProperty("QuickUpStrategyConfigData")]
        public QuickUpStrategyConfigModel QuickUpStrategyConfigData { get; set; }
    }
}
