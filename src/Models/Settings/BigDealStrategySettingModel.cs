using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Settings
{
    public class BigDealStrategySettingModel: ObservableObject
    {
        private int _stockMaxCountEachGroup;
        /// <summary>
        /// 每个分组的股票容量
        /// </summary>
        public int StockMaxCountEachGroup
        {
            get { return _stockMaxCountEachGroup; }
            set { Set(() => StockMaxCountEachGroup, ref _stockMaxCountEachGroup, value); }
        }


        private int _threadCount;
        /// <summary>
        /// 大单策略使用分析数据的线程数量
        /// </summary>
        public int ThreadCount
        {
            get { return _threadCount; }
            set { Set(() => ThreadCount, ref _threadCount, value); }
        }


        private int _forwardSeconds;
        /// <summary>
        /// 分析某一笔时,向前 截取的时间范围
        /// </summary>
        public int ForwardSeconds
        {
            get { return _forwardSeconds; }
            set { Set(() => ForwardSeconds, ref _forwardSeconds, value); }
        }

        private int _afterSeconds;
        /// <summary>
        /// 分析某一笔时,向后 截取的时间范围
        /// </summary>
        public int AfterSeconds
        {
            get { return _afterSeconds; }
            set { Set(() => AfterSeconds, ref _afterSeconds, value); }
        }

        private double _bigDealAmountThreshold;
        /// <summary>
        /// 大单的界限
        /// </summary>
        public double BigDealAmountThreshold
        {
            get { return _bigDealAmountThreshold; }
            set { Set(() => BigDealAmountThreshold, ref _bigDealAmountThreshold, value); }
        }

        private double _bigDealCountThreshold;
        /// <summary>
        /// 在范围内大单的个数界限
        /// </summary>
        public double BigDealCountThreshold
        {
            get { return _bigDealCountThreshold; }
            set { Set(() => BigDealCountThreshold, ref _bigDealCountThreshold, value); }
        }


    }
}
