using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structures.Stocks
{
    public abstract class StockBase
    {
        public string Code { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// 题材
        /// </summary>
        public string Topic { get; set; }
        /// <summary>
        /// 涨幅
        /// </summary>
        public double CurrentPercent { get; set; }

    }
}
