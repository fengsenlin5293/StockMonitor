using StockHelpers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockHelpers
{
    public static class StockHelper
    {
        /// <summary>
        /// 根据股票代码获取股票所属市场
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static StockMarket GetMarket(string code)
        {
            StockMarket result = StockMarket.sz;
            if (string.IsNullOrWhiteSpace(code))
                return result;
            if (code.StartsWith("6"))
                result = StockMarket.sh;

            return result;
        }
    }
}
