using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Crawlers.StockRealTimeDealCrawler
{
    public class WorkTaskModel
    {
        public WorkTaskModel()
        {
            StockCodes = new List<string>();
        }
        public Task WorkTask { get; set; }
        public List<string> StockCodes { get; set; }

        public CancellationTokenSource Cts { get; set; }
    }
}
