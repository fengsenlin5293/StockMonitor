using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structures.JsonModels
{
    public class StorageStockSectionModel
    {
        public StorageStockSectionModel()
        {
            Sections = new List<StorageSection>();
        }
        [JsonProperty("stockCode")]
        public string StockCode { get; set; }

        [JsonProperty("stockName")]
        public string StockName { get; set; }

        [JsonProperty("datas")]
        public List<StorageSection> Sections { get; set; }
    }

    public class StorageSection
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
