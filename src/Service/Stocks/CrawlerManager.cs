using ClawlerInterfaces;
using Crawlers.StockRealTimeDealCrawler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Stocks
{
    public static class CrawlerManager<T>
    {
        public static T GetCrawler()
        {
            Type t = typeof(T);
            if (!t.IsInterface)
                return default(T);

            if (t == typeof(IStockRealTimeDealCrawler))
            {
                return (T)(new StockRealTimeDealCrawler() as IStockRealTimeDealCrawler);
            }
            else
            {
                return default(T);
            }
        }
    }
}
