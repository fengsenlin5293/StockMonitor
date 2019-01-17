using StockAnalysisInterfaces;
using StockAnalysisInterfaces.EventArgs;
using Structures.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Stocks
{
    internal class StockAnalysisService<TStockBase> : IStockAnalysisService<IAnalysisStrategy<TStockBase>, TStockBase> where TStockBase : StockBase
    {
        private IAnalysisStrategy<TStockBase> _analysisStrategy;
        public StockAnalysisService()
        {

        }
        public event StockAnalysisResultUpdatedEventHandler<TStockBase> StockAnalysisResultUpdatedEvent;

        public event EventHandler<int> RemainderCountUpdatedEvent;

        public void StartAnalysis(IAnalysisStrategy<TStockBase> analysisStrategy, IEnumerable<TStockBase> stockBases)
        {
            if (_analysisStrategy != null)
                return;
            _analysisStrategy = analysisStrategy;
            _analysisStrategy.AnalysisResultUpdatedEvent += AnalysisStrategy_AnalysisResultUpdatedEvent;
            _analysisStrategy.RemainderCountUpdatedEvent += AnalysisStrategy_RemainderCountUpdatedEvent;
            _analysisStrategy.Start(stockBases);
        }

        private void AnalysisStrategy_RemainderCountUpdatedEvent(object sender, int e)
        {
            RemainderCountUpdatedEvent?.Invoke(this, e);
        }

        public void StopAnalysis()
        {
            if (_analysisStrategy == null)
                return;
            _analysisStrategy.Stop();
            _analysisStrategy.AnalysisResultUpdatedEvent -= AnalysisStrategy_AnalysisResultUpdatedEvent;
            _analysisStrategy.RemainderCountUpdatedEvent -= AnalysisStrategy_RemainderCountUpdatedEvent;
            _analysisStrategy = null;
        }

        private void AnalysisStrategy_AnalysisResultUpdatedEvent(StockAnalysisResultUpdatedEventArgs<TStockBase> args)
        {
            StockAnalysisResultUpdatedEvent?.Invoke(args);
        }
    }
}
