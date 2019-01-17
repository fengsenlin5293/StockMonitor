using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysisInterfaces.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public class StrategyConfigException : Exception
    {
        private string _message;
        public StrategyConfigException()
        {

        }

        public StrategyConfigException(string message)
        {
            this._message = message;
        }


        public override string Message => this._message;
    }
}
