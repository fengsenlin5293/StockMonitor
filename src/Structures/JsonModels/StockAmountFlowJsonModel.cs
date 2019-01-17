using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structures.JsonModels
{
    public class StockAmountFlowJsonModel
    {
        [JsonProperty("data")]
        public List<string> Datas { get; set; }
    }
}
