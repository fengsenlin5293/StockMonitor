using StockAnalysisInterfaces;
using Structures.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Stocks
{
    public class ServiceManager<TStockBase> where TStockBase : StockBase
    {
        private ServiceManager() { }
        private readonly static ServiceManager<TStockBase> _instance = new ServiceManager<TStockBase>();
        public static ServiceManager<TStockBase> Instance
        {
            get { return _instance; }
        }

        public IStockAnalysisService<IAnalysisStrategy<TStockBase>, TStockBase> GetStockAnalysisService()
        {
            return new StockAnalysisService<TStockBase>();
        }

    }
}
