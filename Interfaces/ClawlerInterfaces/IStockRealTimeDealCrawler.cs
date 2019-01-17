using ClawlerInterfaces.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ClawlerInterfaces
{
    public interface IStockRealTimeDealCrawler
    {
        event StockRealTimeDealUpdatedEventHandler StockRealTimeDealUpdatedEven;

        /// <summary>
        /// 单位:毫秒
        /// </summary>
        /// <param name="interval"></param>
        void StartCrawler(WebProxy webProxy, int interval, IEnumerable<string> stockCodes);

        void StopCrawler();
    }
}
