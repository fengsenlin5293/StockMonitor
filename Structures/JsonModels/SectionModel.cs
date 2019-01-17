using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structures.JsonModels
{
    /// <summary>
    /// 板块的JSON类
    /// </summary>
    public class SectionModel
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("show")]
        public bool Show { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("order")]
        public int Order { get; set; }

        [JsonProperty("groupKey")]
        public string GroupKey { get; set; }

        [JsonProperty("hot")]
        public bool Hot { get; set; }
    }
}
