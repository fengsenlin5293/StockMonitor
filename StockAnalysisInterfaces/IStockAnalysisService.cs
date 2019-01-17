using Structures.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysisInterfaces
{
    public interface IStockAnalysisService<TAnalysisStrategy, TStockBase> where TAnalysisStrategy : IAnalysisStrategy<TStockBase> where TStockBase : StockBase
    {
        event StockAnalysisResultUpdatedEventHandler<TStockBase> StockAnalysisResultUpdatedEvent;
        event EventHandler<int> RemainderCountUpdatedEvent;
        void StartAnalysis(TAnalysisStrategy analysisStrategy, IEnumerable<TStockBase> stockBases);
        void StopAnalysis();
    }
}
