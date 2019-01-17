using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysisInterfaces.EventArgs
{
    public class StockAnalysisResultUpdatedEventArgs<T>
    {
        public StockAnalysisResultUpdatedEventArgs(List<T> results)
        {
            AnalysisResults = new List<T>(results);
        }

        public List<T> AnalysisResults { get; private set; }
    }
}
