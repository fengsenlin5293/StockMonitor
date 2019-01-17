using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structures.JsonModels
{
    public class TopicStockLinkDataModel
    {
        [JsonProperty("SyncTime")]
        public string SyncTime { get; set; }

        [JsonProperty("StorageStockSectionModels")]
        public List<StorageStockSectionModel> StorageStockSectionModels { get; set; }
    }
}
