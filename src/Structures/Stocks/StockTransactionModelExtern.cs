using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structures.Stocks
{
    public class StockTransactionModelExtern : StockTransactionModel
    {
        public StockTransactionModelExtern()
        {

        }

        public StockTransactionModelExtern(StockTransactionModel model) : base(model)
        {
        }
        public double QuickUpPercent { get; set; }
        public override string ToString()
        {
            return $"{Code},{Name},{CurrentPercent}%,[{Time}],{Price},{DealHands},{DealType},{QuickUpPercent}%, [{DateTime.Now.ToString("HH:mm:ss")}],{Topic}";
        }
    }
}
