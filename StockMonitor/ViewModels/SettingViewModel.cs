using CommonHelpers;
using CommonHelpers.Config;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using JsonHelpers;
using Microsoft.Win32;
using Models.Settings;
using Newtonsoft.Json;
using Structures.Messengers.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StockMonitor.ViewModels
{
    /// <summary>
    /// 设置页面的ViewModel
    /// </summary>
    public class SettingViewModel : ViewModelBase
    {
        private string _cacheDataString;
        public SettingViewModel()
        {
            CheckProxyUsefulCommand = new RelayCommand(OnCheckProxyUseful);
            ChooseFromLocalFileCommand = new RelayCommand(OnChooseFromLocalFile);
            ChooseExceptFileCommand = new RelayCommand(OnChooseExceptFile);
            SaveCommand = new RelayCommand(OnSave);
            CancelCommand = new RelayCommand(OnCancel);

            InitData();
            this.PropertyChanged += SettingViewModel_PropertyChanged;
        }

        private void SettingViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var currentString = JsonHelper.SerializeObject(this);
            if (currentString != _cacheDataString)
            {
                CanSave = true;
                CanCancel = true;
            }
            else
            {
                CanSave = false;
                CanCancel = false;
            }
        }


        #region 属性
        /*=================代理=================*/
        private bool _isUseProxy;
        /// <summary>
        /// 是否使用代理
        /// </summary>
        public bool IsUseProxy
        {
            get { return _isUseProxy; }
            set { Set(() => IsUseProxy, ref _isUseProxy, value); }
        }

        private string _ipString;
        /// <summary>
        /// 代理IP
        /// </summary>
        public string IPString
        {
            get { return _ipString; }
            set { Set(() => IPString, ref _ipString, value); }
        }

        private ushort _port;
        /// <summary>
        /// 代理端口
        /// </summary>
        public ushort Port
        {
            get { return _port; }
            set { Set(() => Port, ref _port, value); }
        }

        private string _proxyErrorMessage;
        /// <summary>
        /// 代理错误信息
        /// </summary>
        [JsonIgnore]
        public string ProxyErrorMessage
        {
            get { return _proxyErrorMessage; }
            set { Set(() => ProxyErrorMessage, ref _proxyErrorMessage, value); }
        }

        private bool? _isProxyUsefull;
        /// <summary>
        /// 代理是否可用
        /// </summary>
        [JsonIgnore]
        public bool? IsProxyUsefull
        {
            get { return _isProxyUsefull; }
            set { Set(() => IsProxyUsefull, ref _isProxyUsefull, value); }
        }


        private string _delayMessage;
        /// <summary>
        /// 代理延迟信息
        /// </summary>
        [JsonIgnore]
        public string DelayMessage
        {
            get { return _delayMessage; }
            set { Set(() => DelayMessage, ref _delayMessage, value); }
        }


        /*======================================*/

        /*=================监控范围=================*/

        private bool _isFromConditionFilter;
        /// <summary>
        /// 是否来自于条件筛选
        /// </summary>
        public bool IsFromConditionFilter
        {
            get { return _isFromConditionFilter; }
            set { Set(() => IsFromConditionFilter, ref _isFromConditionFilter, value); }
        }

        private bool _isFromLocalFile;
        /// <summary>
        /// 是否来自于本地文件
        /// </summary>
        public bool IsFromLocalFile
        {
            get { return _isFromLocalFile; }
            set { Set(() => IsFromLocalFile, ref _isFromLocalFile, value); }
        }

        private bool _isFilterPrice;
        /// <summary>
        /// 是否勾选-价格筛选
        /// </summary>
        public bool IsFilterPrice
        {
            get { return _isFilterPrice; }
            set { Set(() => IsFilterPrice, ref _isFilterPrice, value); }
        }

        private double _priceSmall;
        /// <summary>
        /// 最小价格
        /// </summary>
        public double PriceSmall
        {
            get { return _priceSmall; }
            set { Set(() => PriceSmall, ref _priceSmall, value); }
        }

        private double _priceMax;
        /// <summary>
        /// 最大价格
        /// </summary>
        public double PriceMax
        {
            get { return _priceMax; }
            set { Set(() => PriceMax, ref _priceMax, value); }
        }

        private bool _isFilterFlowValue;
        /// <summary>
        /// 是否勾选-流通市值筛选
        /// </summary>
        public bool IsFilterFlowValue
        {
            get { return _isFilterFlowValue; }
            set { Set(() => IsFilterFlowValue, ref _isFilterFlowValue, value); }
        }

        private double _flowValueSmall;
        /// <summary>
        /// 最小流值
        /// </summary>
        public double FlowValueSmall
        {
            get { return _flowValueSmall; }
            set { Set(() => FlowValueSmall, ref _flowValueSmall, value); }
        }

        private double _flowValueMax;
        /// <summary>
        /// 最大流值
        /// </summary>
        public double FlowValueMax
        {
            get { return _flowValueMax; }
            set { Set(() => FlowValueMax, ref _flowValueMax, value); }
        }

        private bool _isFilterTopic;
        /// <summary>
        /// 是否勾选-题材筛选
        /// </summary>
        public bool IsFilterTopic
        {
            get { return _isFilterTopic; }
            set { Set(() => IsFilterTopic, ref _isFilterTopic, value); }
        }

        private bool _isLogicAnd;
        /// <summary>
        /// 多个题材之间是“与”还是“或”
        /// true:与, false:或
        /// </summary>
        public bool IsLogicAnd
        {
            get { return _isLogicAnd; }
            set { Set(() => IsLogicAnd, ref _isLogicAnd, value); }
        }

        private string _filterTopicText;
        /// <summary>
        /// 筛选的题材内容
        /// </summary>
        public string FilterTopicText
        {
            get { return _filterTopicText; }
            set { Set(() => FilterTopicText, ref _filterTopicText, value); }
        }


        private string _monitorFilePath;
        /// <summary>
        /// 来自于文件的文件路径
        /// </summary>
        public string MonitorFilePath
        {
            get { return _monitorFilePath; }
            set { Set(() => MonitorFilePath, ref _monitorFilePath, value); }
        }

        /*==========================================*/



        /*=================不监控范围=================*/

        private bool _isExceptMonitorRange;

        /// <summary>
        /// 是否排除一些监控范围
        /// </summary>
        public bool IsExceptMonitorRange
        {
            get { return _isExceptMonitorRange; }
            set { Set(() => IsExceptMonitorRange, ref _isExceptMonitorRange, value); }
        }

        private string _exceptMonitorFilePath;

        public string ExceptMonitorFilePath
        {
            get { return _exceptMonitorFilePath; }
            set { Set(() => ExceptMonitorFilePath, ref _exceptMonitorFilePath, value); }
        }

        /*==========================================*/



        /*=================监控策略=================*/

        private bool _isUseBigOrderStrategy;
        /// <summary>
        /// 是否使用'大单'策略
        /// </summary>
        public bool IsUseBigOrderStrategy
        {
            get { return _isUseBigOrderStrategy; }
            set { Set(() => IsUseBigOrderStrategy, ref _isUseBigOrderStrategy, value); }
        }

        private bool _isUseRapidRiseStrategy;
        /// <summary>
        /// 是否使用'快速上涨'策略
        /// </summary>
        public bool IsUseRapidRiseStrategy
        {
            get { return _isUseRapidRiseStrategy; }
            set { Set(() => IsUseRapidRiseStrategy, ref _isUseRapidRiseStrategy, value); }
        }


        /*==========================================*/

        /*=================高级设置=================*/

        private ClawlerSettingModel _clawlerSettingData;
        /// <summary>
        /// 爬虫高级设置
        /// </summary>
        public ClawlerSettingModel ClawlerSettingData
        {
            get { return _clawlerSettingData; }
            set { Set(() => ClawlerSettingData, ref _clawlerSettingData, value); }
        }

        private BigDealStrategySettingModel _bigDealStrategySettingData;
        /// <summary>
        /// 大单策略高级设置
        /// </summary>
        public BigDealStrategySettingModel BigDealStrategySettingData
        {
            get { return _bigDealStrategySettingData; }
            set { Set(() => BigDealStrategySettingData, ref _bigDealStrategySettingData, value); }
        }

        private QuickUpStrategySettingModel _quickUpStrategySettingData;
        /// <summary>
        /// 快速上涨策略高级设置
        /// </summary>
        public QuickUpStrategySettingModel QuickUpStrategySettingData
        {
            get { return _quickUpStrategySettingData; }
            set { Set(() => QuickUpStrategySettingData, ref _quickUpStrategySettingData, value); }
        }


        /*==========================================*/

        /*=================保存/取消=================*/
        private bool _canSave;
        [JsonIgnore]
        public bool CanSave
        {
            get { return _canSave; }
            set { Set(() => CanSave, ref _canSave, value); }
        }

        private bool _canCancel;
        [JsonIgnore]
        public bool CanCancel
        {
            get { return _canCancel; }
            set { Set(() => CanCancel, ref _canCancel, value); }
        }


        /*==========================================*/

        #endregion

        #region Commands

        public ICommand CheckProxyUsefulCommand { get; set; }
        public ICommand ChooseFromLocalFileCommand { get; set; }
        public ICommand ChooseExceptFileCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        #endregion


        #region Command Execute
        /// <summary>
        /// 校验代理是否可用
        /// </summary>
        private async void OnCheckProxyUseful()
        {
            IsProxyUsefull = null;
            this.DelayMessage = string.Empty;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var checkResult = await Task.Run(() => ProxyHelper.CheckProxyUsefull(this.IPString, this.Port, 10000));
            stopwatch.Stop();

            if (!checkResult)
            {
                IsProxyUsefull = false;
                this.DelayMessage = string.Empty;
                return;
            }

            IsProxyUsefull = true;
            this.DelayMessage = $"{ResourceHelper.FindKey("Setting_CostTime")}: {stopwatch.ElapsedMilliseconds}ms";
        }

        /// <summary>
        /// 选择来自于本地的文件
        /// </summary>
        private void OnChooseFromLocalFile()
        {
            if (GetSelectedFilePath(out var filePath))
                this.MonitorFilePath = filePath;
        }

        /// <summary>
        /// 选择排除监控的文件路径
        /// </summary>
        private void OnChooseExceptFile()
        {
            if (GetSelectedFilePath(out var filePath))
                this.ExceptMonitorFilePath = filePath;
        }

        /// <summary>
        /// 保存
        /// </summary>
        private void OnSave()
        {
            var config = ConfigJsonHelper.GetConfigModel();
            if (config == null) return;
            config.IsUseProxy = IsUseProxy;
            config.ProxyData.IP = IPString;
            config.ProxyData.Port = Port;

            config.IsFromConditionFilter = IsFromConditionFilter;
            config.FromConditionFilterData.IsFilterPrice = IsFilterPrice;
            config.FromConditionFilterData.PriceSmall = PriceSmall;
            config.FromConditionFilterData.PriceMax = PriceMax;

            config.FromConditionFilterData.IsFilterFlowValue = IsFilterFlowValue;
            config.FromConditionFilterData.FlowValueSmall = FlowValueSmall;
            config.FromConditionFilterData.FlowValueMax = FlowValueMax;

            config.FromConditionFilterData.IsFilterTopic = IsFilterTopic;
            config.FromConditionFilterData.IsLogicAnd = IsLogicAnd;
            config.FromConditionFilterData.FilterTopicText = FilterTopicText;

            config.IsFromLocalFile = IsFromLocalFile;
            config.MonitorFilePath = MonitorFilePath;

            config.IsExceptMonitorRange = IsExceptMonitorRange;
            config.ExceptMonitorFilePath = ExceptMonitorFilePath;

            config.IsUseBigOrderStrategy = IsUseBigOrderStrategy;
            config.IsUseRapidRiseStrategy = IsUseRapidRiseStrategy;

            config.ClawlerConfigData.CodeCountPerThread = ClawlerSettingData.CodeCountPerThread;
            config.ClawlerConfigData.QueryCountPerTime = ClawlerSettingData.QueryCountPerTime;
            config.ClawlerConfigData.QueryInterval = ClawlerSettingData.QueryInterval;

            config.BigDealStrategyConfigData.StockMaxCountEachGroup = BigDealStrategySettingData.StockMaxCountEachGroup;
            config.BigDealStrategyConfigData.ThreadCount = BigDealStrategySettingData.ThreadCount;
            config.BigDealStrategyConfigData.ForwardSeconds = BigDealStrategySettingData.ForwardSeconds;
            config.BigDealStrategyConfigData.AfterSeconds = BigDealStrategySettingData.AfterSeconds;
            config.BigDealStrategyConfigData.BigDealAmountThreshold = BigDealStrategySettingData.BigDealAmountThreshold;
            config.BigDealStrategyConfigData.BigDealCountThreshold = BigDealStrategySettingData.BigDealCountThreshold;

            config.QuickUpStrategyConfigData.StockMaxCountEachGroup = QuickUpStrategySettingData.StockMaxCountEachGroup;
            config.QuickUpStrategyConfigData.ThreadCount = QuickUpStrategySettingData.ThreadCount;
            config.QuickUpStrategyConfigData.ForwardSeconds = QuickUpStrategySettingData.ForwardSeconds;
            config.QuickUpStrategyConfigData.AfterSeconds = QuickUpStrategySettingData.AfterSeconds;
            config.QuickUpStrategyConfigData.DealAmountThreshold = QuickUpStrategySettingData.DealAmountThreshold;
            config.QuickUpStrategyConfigData.QuickUpThreshold = QuickUpStrategySettingData.QuickUpThreshold;

            ConfigJsonHelper.SaveConfig();

            _cacheDataString = JsonHelper.SerializeObject(this);
            this.PropertyChanged -= SettingViewModel_PropertyChanged;
            CanSave = false;
            CanCancel = false;
            this.PropertyChanged += SettingViewModel_PropertyChanged;

            //停止正在进行的监控
            Messenger.Default.Send(string.Empty, MonitorOptions.StopMonitor);
        }

        /// <summary>
        /// 取消
        /// </summary>
        private void OnCancel()
        {
            this.PropertyChanged -= SettingViewModel_PropertyChanged;

            var obj = JsonHelper.DeserializeJsonToObject<SettingViewModel>(_cacheDataString);
            
            //避免内存泄漏
            obj.UnSubscribe();

            CloneProperty(obj, this, typeof(SettingViewModel), new[] { typeof(ClawlerSettingModel), typeof(BigDealStrategySettingModel), typeof(QuickUpStrategySettingModel) });
            CloneProperty(obj.ClawlerSettingData, this.ClawlerSettingData, typeof(ClawlerSettingModel));
            CloneProperty(obj.BigDealStrategySettingData, this.BigDealStrategySettingData, typeof(BigDealStrategySettingModel));
            CloneProperty(obj.QuickUpStrategySettingData, this.QuickUpStrategySettingData, typeof(QuickUpStrategySettingModel));

            CanSave = false;
            CanCancel = false;
            this.PropertyChanged += SettingViewModel_PropertyChanged;
        }



        #endregion

        #region private methods
        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitData()
        {
            ConfigJsonHelper.LoadedAppConfig();
            var config = ConfigJsonHelper.GetConfigModel();
            if (config == null) return;
            IsUseProxy = config.IsUseProxy;
            IPString = config.ProxyData.IP;
            Port = (ushort)config.ProxyData.Port;

            IsFromConditionFilter = config.IsFromConditionFilter;
            IsFilterPrice = config.FromConditionFilterData.IsFilterPrice;
            PriceSmall = config.FromConditionFilterData.PriceSmall;
            PriceMax = config.FromConditionFilterData.PriceMax;

            IsFilterFlowValue = config.FromConditionFilterData.IsFilterFlowValue;
            FlowValueSmall = config.FromConditionFilterData.FlowValueSmall;
            FlowValueMax = config.FromConditionFilterData.FlowValueMax;

            IsFilterTopic = config.FromConditionFilterData.IsFilterTopic;
            IsLogicAnd = config.FromConditionFilterData.IsLogicAnd;
            FilterTopicText = config.FromConditionFilterData.FilterTopicText;

            IsFromLocalFile = config.IsFromLocalFile;
            MonitorFilePath = config.MonitorFilePath;

            IsExceptMonitorRange = config.IsExceptMonitorRange;
            ExceptMonitorFilePath = config.ExceptMonitorFilePath;

            IsUseBigOrderStrategy = config.IsUseBigOrderStrategy;
            IsUseRapidRiseStrategy = config.IsUseRapidRiseStrategy;

            ClawlerSettingData = new ClawlerSettingModel();
            ClawlerSettingData.CodeCountPerThread = config.ClawlerConfigData.CodeCountPerThread;
            ClawlerSettingData.QueryCountPerTime = config.ClawlerConfigData.QueryCountPerTime;
            ClawlerSettingData.QueryInterval = config.ClawlerConfigData.QueryInterval;

            BigDealStrategySettingData = new BigDealStrategySettingModel();
            BigDealStrategySettingData.StockMaxCountEachGroup = config.BigDealStrategyConfigData.StockMaxCountEachGroup;
            BigDealStrategySettingData.ThreadCount = config.BigDealStrategyConfigData.ThreadCount;
            BigDealStrategySettingData.ForwardSeconds = config.BigDealStrategyConfigData.ForwardSeconds;
            BigDealStrategySettingData.AfterSeconds = config.BigDealStrategyConfigData.AfterSeconds;
            BigDealStrategySettingData.BigDealAmountThreshold = config.BigDealStrategyConfigData.BigDealAmountThreshold;
            BigDealStrategySettingData.BigDealCountThreshold = config.BigDealStrategyConfigData.BigDealCountThreshold;

            QuickUpStrategySettingData = new QuickUpStrategySettingModel();
            QuickUpStrategySettingData.StockMaxCountEachGroup = config.QuickUpStrategyConfigData.StockMaxCountEachGroup;
            QuickUpStrategySettingData.ThreadCount = config.QuickUpStrategyConfigData.ThreadCount;
            QuickUpStrategySettingData.ForwardSeconds = config.QuickUpStrategyConfigData.ForwardSeconds;
            QuickUpStrategySettingData.AfterSeconds = config.QuickUpStrategyConfigData.AfterSeconds;
            QuickUpStrategySettingData.DealAmountThreshold = config.QuickUpStrategyConfigData.DealAmountThreshold;
            QuickUpStrategySettingData.QuickUpThreshold = config.QuickUpStrategyConfigData.QuickUpThreshold;

            ClawlerSettingData.PropertyChanged += SettingViewModel_PropertyChanged;
            BigDealStrategySettingData.PropertyChanged += SettingViewModel_PropertyChanged;
            QuickUpStrategySettingData.PropertyChanged += SettingViewModel_PropertyChanged;

            _cacheDataString = JsonHelper.SerializeObject(this);
        }

        /// <summary>
        /// 获取所选文件路径
        /// </summary>
        /// <returns></returns>
        private bool GetSelectedFilePath(out string filePath)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Filter = "(.txt)|*.txt";
            var dlgResult = openFileDialog.ShowDialog();
            if (dlgResult != true)
            {
                filePath = string.Empty;
                return false;
            }
            filePath = openFileDialog.FileName;
            return true;
        }

        private void CloneProperty(object source, object target, Type type, params Type[] excptPropertyTypes)
        {
            if (source == null || target == null)
                return;

            var properties = type.GetProperties();
            foreach (var item in properties)
            {
                if (item.PropertyType == typeof(ICommand))
                    continue;
                if (excptPropertyTypes != null && excptPropertyTypes.Contains(item.PropertyType))
                    continue;

                if (item.DeclaringType != type)
                    continue;
                var value = item.GetValue(source);
                item.SetValue(target, value);
            }
        }

        private void UnSubscribe()
        {
            ClawlerSettingData.PropertyChanged -= SettingViewModel_PropertyChanged;
            BigDealStrategySettingData.PropertyChanged -= SettingViewModel_PropertyChanged;
            QuickUpStrategySettingData.PropertyChanged -= SettingViewModel_PropertyChanged;
            this.PropertyChanged -= SettingViewModel_PropertyChanged;
        }

        #endregion
    }
}
