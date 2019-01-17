using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structures.JsonModels
{
    public class QueryStockDetailResponseModel
    {
        [JsonProperty("data")]
        public List<string> Datas { get; set; }

        [JsonProperty("recordsFiltered")]
        public int RecordsFiltered { get; set; }
    }
}
