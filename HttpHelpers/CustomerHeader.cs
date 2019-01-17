using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpHelpers
{
    public class CustomerHeader
    {
        private string _defaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.75 Safari/537.36";
        public CustomerHeader()
        {
            UserAgent = _defaultUserAgent;
        }
        public string UserAgent { get; set; }

        public string Referer { get; set; } = string.Empty;
    }
}
