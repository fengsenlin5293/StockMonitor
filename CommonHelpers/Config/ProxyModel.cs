using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelpers.Config
{
    public class ProxyModel
    {
        [JsonProperty("IP")]
        public string IP { get;  set; }
        [JsonProperty("Port")]
        public int Port { get;  set; }
    
    }
}
