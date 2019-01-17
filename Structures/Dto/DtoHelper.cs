using Structures.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structures.Dto
{
    public static class DtoHelper
    {
        public static List<StockTransactionModel> GetStockTransactionModels(List<string> datas)
        {
            int count = 8;
            List<StockTransactionModel> result = new List<StockTransactionModel>();
            try
            {
                datas.ForEach(data =>
                {
                    var arrs = data.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    if (arrs.Count() == count)
                    {

                        StockTransactionModel model = new StockTransactionModel();
                        model.Time = arrs[0];
                        model.Price = double.Parse(arrs[1]);
                        model.DealHands = double.Parse(arrs[2]);
                        model.DealType = (BuyOrSale)int.Parse(arrs[3]);
                        model.Status = (StockStatus)int.Parse(arrs[4]);
                        model.DealHands2 = double.Parse(arrs[5]);
                        model.Unknow2 = int.Parse(arrs[6]);
                        model.Unknow3 = int.Parse(arrs[7]);
                        result.Add(model);
                    }
                });
            }
            catch
            {

            }
            return result.OrderBy(item => item.Time).ToList();
        }

        public static List<StockDetailModel> GetStockDetailModel(List<string> datas)
        {
            int count = 26;
            List<StockDetailModel> result = new List<StockDetailModel>();
            try
            {
                datas.ForEach(data =>
                {
                    try
                    {
                        var arrs = data.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        if (arrs.Count() == count)
                        {
                            StockDetailModel model = new StockDetailModel();
                            model.Code = arrs[1];
                            model.Name = arrs[2];
                            //try
                            //{
                            model.Price = double.Parse(arrs[3]);
                            //}
                            //catch
                            //{

                            //}
                            //try
                            //{
                            model.ChangedPrice = double.Parse(arrs[4]);
                            //}
                            //catch
                            //{

                            //}
                            //try
                            //{
                            model.ChangedPercent = double.Parse(arrs[5]);
                            //}
                            //catch
                            //{

                            //}
                            //try
                            //{
                            model.TodayTotalHand = double.Parse(arrs[6]);
                            //}
                            //catch
                            //{

                            //}
                            //try
                            //{
                            model.TodayAmount = double.Parse(arrs[7]);
                            //}
                            //catch
                            //{

                            //}
                            //try
                            //{
                            model.YesterdayEndPrice = double.Parse(arrs[12]);
                            //}
                            //catch
                            //{

                            //}
                            //try
                            //{
                            model.TotalValue = double.Parse(arrs[18]);
                            //}
                            //catch
                            //{

                            //}

                            //try
                            //{
                            model.FlowlValue = double.Parse(arrs[19]);
                            //}
                            //catch
                            //{

                            //}
                            result.Add(model);
                        }
                    }
                    catch
                    {
                        return;
                    }
                });
            }
            catch
            {

            }
            return result;
        }

        public static List<StockAmountFlowModel> GetStockAmountFlowModel(List<string> datas)
        {
            int count = 17;
            List<StockAmountFlowModel> result = new List<StockAmountFlowModel>();
            try
            {
                datas.ForEach(data =>
                {
                    try
                    {
                        var arrs = data.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        if (arrs.Count() == count)
                        {
                            StockAmountFlowModel model = new StockAmountFlowModel();
                            model.Code = arrs[1];
                            model.Name = arrs[2];
                            model.Price = double.Parse(arrs[3]);
                            model.CurrentPercent = double.Parse(arrs[4]);

                            model.RealMainInAmount = double.Parse(arrs[5]);
                            model.RealMainInAmountPercent = double.Parse(arrs[6]);

                            model.RealSupperBigMainInAmount = double.Parse(arrs[7]);
                            model.RealSupperBigMainInAmountPercent = double.Parse(arrs[8]);

                            model.RealBigMainInAmount = double.Parse(arrs[9]);
                            model.RealBigMainInAmountPercent = double.Parse(arrs[10]);

                            model.RealMiddleMainInAmount = double.Parse(arrs[11]);
                            model.RealMiddleMainInAmountPercent = double.Parse(arrs[12]);

                            model.RealSmallMainInAmount = double.Parse(arrs[13]);
                            model.RealSmallMainInAmountPercent = double.Parse(arrs[14]);


                            result.Add(model);
                        }
                    }
                    catch
                    {
                        return;
                    }
                });
            }
            catch
            {

            }
            return result;
        }
        private static void SetDoubleValue(ref double source, string str)
        {
            double value;
            bool parseSuc = double.TryParse(str, out value);
            if (parseSuc)
            {
                source = value;
            }
        }
    }
}
