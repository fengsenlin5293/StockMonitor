using Structures.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.ClawlerInterfaces.EventArgs
{
    public class StockRealTimeDealUpdatedEventArgs
    {
        public StockRealTimeDealUpdatedEventArgs(IEnumerable<StockTransactionModel> addedModels)
        {
            AddedItems = new List<StockTransactionModel>(addedModels);
        }

        public List<StockTransactionModel> AddedItems { get; }
    }
}
