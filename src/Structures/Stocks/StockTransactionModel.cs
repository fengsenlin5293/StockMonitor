using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structures.Stocks
{
    public class StockTransactionModel : StockBase
    {
        public StockTransactionModel()
        {

        }
        public StockTransactionModel(StockTransactionModel model)
        {
            this.Code = model.Code;
            this.Name = model.Name;
            this.Time = model.Time;
            this.Price = model.Price;
            this.DealHands = model.DealHands;
            this.DealType = model.DealType;
            this.Status = model.Status;
            this.DealHands2 = model.DealHands2;
            this.Unknow2 = model.Unknow2;
            this.Unknow3 = model.Unknow3;

        }

        public string Time { get; set; }

        public double Price { get; set; }

        public double DealHands { get; set; }

        /// <summary>
        /// 买盘还是卖盘
        /// </summary>
        public BuyOrSale DealType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public StockStatus Status { get; set; }

        public double DealHands2 { get; set; }

        public int Unknow2 { get; set; }

        public int Unknow3 { get; set; }

        public override string ToString()
        {
            return $"{Code},{Name},{CurrentPercent}%,[{Time}],{Price},{DealHands},{DealType},{Status},[{DateTime.Now.ToString("HH:mm:ss")}],{DealHands2},{Unknow2},{Unknow3},{Topic}";
        }
    }
}
