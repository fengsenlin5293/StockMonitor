using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelpers.Config
{
    public class FromConditionFilterModel
    {
        [JsonProperty("IsFilterPrice")]
        public bool IsFilterPrice { get; set; }

        [JsonProperty("PriceSmall")]
        public double PriceSmall { get; set; }

        [JsonProperty("PriceMax")]
        public double PriceMax { get; set; }

        [JsonProperty("IsFilterFlowValue")]
        public bool IsFilterFlowValue { get; set; }

        [JsonProperty("FlowValueSmall")]
        public double FlowValueSmall { get; set; }

        [JsonProperty("FlowValueMax")]
        public double FlowValueMax { get; set; }

        [JsonProperty("IsFilterTopic")]
        public bool IsFilterTopic { get; set; }

        [JsonProperty("IsLogicAnd")]
        public bool IsLogicAnd { get; set; }

        [JsonProperty("FilterTopicText")]
        public string FilterTopicText { get; set; }
    }
}
