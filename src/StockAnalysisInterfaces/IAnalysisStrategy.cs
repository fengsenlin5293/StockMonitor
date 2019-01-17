using StockAnalysisInterfaces.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysisInterfaces
{
    public delegate void StockAnalysisResultUpdatedEventHandler<T>(StockAnalysisResultUpdatedEventArgs<T> args);

    public interface IAnalysisStrategy<TStockBase>
    {
        event StockAnalysisResultUpdatedEventHandler<TStockBase> AnalysisResultUpdatedEvent;
        event EventHandler<int> RemainderCountUpdatedEvent;
        void Start(IEnumerable<TStockBase> stockBases);
        void Stop();
    }
}
