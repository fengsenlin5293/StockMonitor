using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structures.JsonModels
{
    public class QueryStockTransactionResponseModel
    {
        [JsonProperty("result")]
        public bool Result { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("value")]
        public QueryStockTransactionResponseValueModel Value { get; set; }
    }

    public class QueryStockTransactionResponseValueModel
    {
        [JsonProperty("pc")]
        public string Pc { get; set; }

        [JsonProperty("data")]
        public List<string> Datas { get; set; }
    }
}
