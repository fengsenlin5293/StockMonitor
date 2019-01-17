using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Settings
{
    public class QuickUpStrategySettingModel: ObservableObject
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

        private double _dealAmountThreshold;
        /// <summary>
        /// 筛选数据时的成交金额的最低界限
        /// </summary>
        public double DealAmountThreshold
        {
            get { return _dealAmountThreshold; }
            set { Set(() => DealAmountThreshold, ref _dealAmountThreshold, value); }
        }

        private double _quickUpThreshold;
        /// <summary>
        /// 快速上涨幅度的界限
        /// </summary>
        public double QuickUpThreshold
        {
            get { return _quickUpThreshold; }
            set { Set(() => QuickUpThreshold, ref _quickUpThreshold, value); }
        }


    }
}
