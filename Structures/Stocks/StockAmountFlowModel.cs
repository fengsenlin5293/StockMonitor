using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structures.Stocks
{
    public class StockAmountFlowModel : StockBase
    {
        public double Price { get; set; } = 0.001;
        public double ChangedPercent { get; set; } = 0.001;
        /// <summary>
        /// 单位 万
        /// </summary>
        public double RealMainInAmount { get; set; } = 0.001;
        public double RealMainInAmountPercent { get; set; } = 0.001;

        public double RealSupperBigMainInAmount { get; set; } = 0.001;
        public double RealSupperBigMainInAmountPercent { get; set; } = 0.001;

        public double RealBigMainInAmount { get; set; } = 0.001;
        public double RealBigMainInAmountPercent { get; set; } = 0.001;

        public double RealMiddleMainInAmount { get; set; } = 0.001;
        public double RealMiddleMainInAmountPercent { get; set; } = 0.001;

        public double RealSmallMainInAmount { get; set; } = 0.001;
        public double RealSmallMainInAmountPercent { get; set; } = 0.001;
    }
}
