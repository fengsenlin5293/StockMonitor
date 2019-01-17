using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Settings
{
    public class ClawlerSettingModel : ObservableObject
    {

        #region properties

        private int _codeCountPerThread;
        /// <summary>
        /// 每个线程处理的个股数量
        /// </summary>
        public int CodeCountPerThread
        {
            get { return _codeCountPerThread; }
            set { Set(() => CodeCountPerThread, ref _codeCountPerThread, value); }
        }

        private int _queryCountPerTime;
        /// <summary>
        /// 每次请求的数量
        /// </summary>
        public int QueryCountPerTime
        {
            get { return _queryCountPerTime; }
            set { Set(() => QueryCountPerTime, ref _queryCountPerTime, value); }
        }


        private int _queryInterval;
        /// <summary>
        /// 每次请求时间间隔(ms)
        /// </summary>
        public int QueryInterval
        {
            get { return _queryInterval; }
            set { Set(() => QueryInterval, ref _queryInterval, value); }
        }


        #endregion

    }
}
