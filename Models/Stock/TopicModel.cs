using GalaSoft.MvvmLight;
using Structures.Attibutes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Stock
{
    public class TopicModel : ObservableObject
    {
        private string _topic;
        [ColumnDescription("Monitor_TableHeader_Topic", nameof(Topic))]
        public string Topic
        {
            get { return _topic; }
            set { Set(() => Topic, ref _topic, value); }
        }

        private string _urlStr;
        //[ColumnDescription("超链接", nameof(UrlStr))]
        public string UrlStr
        {
            get { return _urlStr; }
            set { Set(() => UrlStr, ref _urlStr, value); }
        }
    }
}
