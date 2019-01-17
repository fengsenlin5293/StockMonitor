using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structures.Stocks
{
    /// <summary>
    /// 股票涨跌状态
    /// </summary>
    public enum StockStatus
    {
        Up = 1,
        Same = 0,
        Down = -1
    }

    /// <summary>
    /// 买盘还是卖盘
    /// </summary>
    public enum BuyOrSale
    {
        Sale = 1,
        Buy = 2
    }
}
