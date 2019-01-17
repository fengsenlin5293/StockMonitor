using CommonHelpers;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Structures.Messengers.Args;
using Structures.Messengers.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace StockMonitor.ViewModels
{
    public class StatusBarVeiwModel : ViewModelBase
    {
        private Timer _systemTimer;

        public StatusBarVeiwModel()
        {
            Subscribe();
            Init();
        }



        #region properties

        private DateTime _systemTime;
        /// <summary>
        /// 系统时间
        /// </summary>
        public DateTime SystemTime
        {
            get { return _systemTime; }
            set { Set(() => SystemTime, ref _systemTime, value); }
        }


        private string _currentStatusMessage;
        /// <summary>
        /// 当前的状态消息
        /// </summary>
        public string CurrentStatusMessage
        {
            get { return _currentStatusMessage; }
            set { Set(() => CurrentStatusMessage, ref _currentStatusMessage, value); }
        }


        #endregion

        #region private methods

        private void Subscribe()
        {
            Messenger.Default.Register<StatusBarArgs>(this, StatusBarToken.UpdateStatus, OnUpdateStatus);
        }

        private void OnUpdateStatus(StatusBarArgs args)
        {
            if (args == null)
                return;
            if (args.IsBusy)
                this.CurrentStatusMessage = args.Message;
            else
                this.CurrentStatusMessage = ResourceHelper.FindKey("StatusBar_IsFree");
        }

        private void Init()
        {
            _systemTimer = new Timer();
            _systemTimer.Interval = 1000;
            _systemTimer.Elapsed += SystemTimer_Elapsed;
            _systemTimer.Start();

            SystemTime = DateTime.Now;
            this.CurrentStatusMessage = ResourceHelper.FindKey("StatusBar_IsFree");
        }

        private void SystemTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SystemTime = DateTime.Now;
        }

        #endregion
    }
}
