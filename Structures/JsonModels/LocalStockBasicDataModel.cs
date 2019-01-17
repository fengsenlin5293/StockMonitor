using Newtonsoft.Json;
using Structures.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structures.JsonModels
{
    public class LocalStockBasicDataModel
    {
        [JsonProperty("SyncTime")]
        public string SyncTime { get; set; }

        [JsonProperty("StockDetailModels")]
        public List<StockDetailModel> StockDetailModels { get; set; }
    }
}
