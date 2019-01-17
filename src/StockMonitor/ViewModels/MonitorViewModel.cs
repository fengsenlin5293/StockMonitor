using AnalysisStrategys.BigDealAnalysisStrategy;
using AnalysisStrategys.QuickUpAnalysisStrategy;
using ClawlerInterfaces;
using ClawlerInterfaces.Exceptions;
using CommonHelpers;
using CommonHelpers.Config;
using DotNet.Log4Net;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Interfaces.ClawlerInterfaces.EventArgs;
using JsonHelpers;
using Models.Stock;
using Service.Stocks;
using StockAnalysisInterfaces;
using StockAnalysisInterfaces.EventArgs;
using StockAnalysisInterfaces.Exceptions;
using StockHelpers;
using Structures;
using Structures.JsonModels;
using Structures.Messengers.Args;
using Structures.Messengers.Tokens;
using Structures.Stocks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StockMonitor.ViewModels
{
    public class MonitorViewModel : ViewModelBase
    {

        #region fields
        private string _stockMaketOpenTime = "09:30:00";
        private ConfigJsonModel _configModel;
        private IStockRealTimeDealCrawler _iStockRealTimeDealCrawler;
        private int _count = 0;
        IStockAnalysisService<IAnalysisStrategy<StockTransactionModel>, StockTransactionModel> _stockAnalysisService;
        IStockAnalysisService<IAnalysisStrategy<StockTransactionModelExtern>, StockTransactionModelExtern> _quickAnalysisService;

        /// <summary>
        /// 大单策略数据源缓存
        /// </summary>
        private ConcurrentQueue<StockTransactionModel> _cacheBigDealAnalysisStrategy = new ConcurrentQueue<StockTransactionModel>();

        /// <summary>
        /// 快速上涨策略数据源缓存
        /// </summary>
        private ConcurrentQueue<StockTransactionModelExtern> _cacheQuickUpAnalysisStrategy = new ConcurrentQueue<StockTransactionModelExtern>();

        #endregion

        public MonitorViewModel()
        {
            StartCommand = new RelayCommand(OnStart);
            StopCommand = new RelayCommand(OnStop);
            _stockModels = new ObservableCollection<StockModel>();
            _stockModels.CollectionChanged += _stockModels_CollectionChanged;

            _iStockRealTimeDealCrawler = CrawlerManager<IStockRealTimeDealCrawler>.GetCrawler();
            if (_iStockRealTimeDealCrawler != null)
                _iStockRealTimeDealCrawler.StockRealTimeDealUpdatedEven += StockRealTimeDealCrawler_StockRealTimeDealUpdatedEven;

            this.TotalCrawledCount = 0;

            InitData();
            Subscribe();
        }


        #region properties

        private bool _isRunning;
        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning
        {
            get { return _isRunning; }
            set { Set(() => IsRunning, ref _isRunning, value); }
        }


        private bool _canStart;
        /// <summary>
        /// 是否可以开始监控
        /// </summary>
        public bool CanStart
        {
            get { return _canStart; }
            set
            {
                Set(() => CanStart, ref _canStart, value);
                IsRunning = !value;
            }
        }

        private bool _canStop;
        /// <summary>
        /// 是否可以停止监控
        /// </summary>
        public bool CanStop
        {
            get { return _canStop; }
            set { Set(() => CanStop, ref _canStop, value); }
        }

        private int _monitoringStockCount;
        /// <summary>
        /// 正在监控股票数量
        /// </summary>
        public int MonitoringStockCount
        {
            get { return _monitoringStockCount; }
            set { Set(() => MonitoringStockCount, ref _monitoringStockCount, value); }
        }

        private double _totalCrawledCount;
        /// <summary>
        /// 爬取到的成交数
        /// </summary>
        public double TotalCrawledCount
        {
            get { return _totalCrawledCount; }
            set { Set(() => TotalCrawledCount, ref _totalCrawledCount, value); }
        }

        private double _bigDealRemainCount;
        /// <summary>
        /// 大单策略待处理的数量
        /// </summary>
        public double BigDealRemainCount
        {
            get { return _bigDealRemainCount; }
            set { Set(() => BigDealRemainCount, ref _bigDealRemainCount, value); }
        }

        private double _quickRemainCount;
        /// <summary>
        /// 快速上涨策略待处理的数量
        /// </summary>
        public double QuickRemainCount
        {
            get { return _quickRemainCount; }
            set { Set(() => QuickRemainCount, ref _quickRemainCount, value); }
        }


        private ObservableCollection<StockModel> _stockModels;

        public ObservableCollection<StockModel> StockModels
        {
            get { return _stockModels; }
            set { Set(() => StockModels, ref _stockModels, value); }
        }

        private ObservableCollection<StockModel> _pageStockModels = new ObservableCollection<StockModel>();

        public ObservableCollection<StockModel> PageStockModels
        {
            get { return _pageStockModels; }
            set { Set(() => PageStockModels, ref _pageStockModels, value); }
        }


        private int _currentPageIndex = 1;

        public int CurrentPageIndex
        {
            get { return _currentPageIndex; }
            set
            {
                _currentPageIndex = value;
                ChangePageTo(value);
                RaisePropertyChanged(() => CurrentPageIndex);
            }
        }

        private int _pageSize = 50;

        public int PageSize
        {
            get { return _pageSize; }
        }


        #endregion

        #region Commands

        public ICommand StartCommand { get; set; }
        public ICommand StopCommand { get; set; }

        #endregion

        #region Command Execute
        /// <summary>
        /// 开始监控
        /// </summary>
        private async void OnStart()
        {
            this.CanStart = false;
            StockModels.Clear();
            PageStockModels.Clear();
            _count = 0;
            this.TotalCrawledCount = 0;
            this.BigDealRemainCount = 0;
            this.QuickRemainCount = 0;
            StockModel.StaticIndex = 1;
            try
            {
                _configModel = ConfigJsonHelper.GetConfigModel();
                Messenger.Default.Send(new StatusBarArgs(true, ResourceHelper.FindKey("StatusBar_Message_CheckingConfig")), StatusBarToken.UpdateStatus);
                bool isConfigUseful = await Task.Run(() => CheckConfig());
                if (!isConfigUseful)
                {
                    this.CanStart = true;
                    return;
                }

                Messenger.Default.Send(new StatusBarArgs(true, ResourceHelper.FindKey("StatusBar_Message_FilteringStock")), StatusBarToken.UpdateStatus);
                List<string> monitorStocks = GetMonitorStocks();

                if (monitorStocks == null)
                {
                    this.CanStart = true;
                    return;
                }

                if (!monitorStocks.Any())
                {
                    Messenger.Default.Send(new StatusBarArgs(true, ResourceHelper.FindKey("StatusBar_ConfigError_FilterNone")), StatusBarToken.UpdateStatus);
                    this.CanStart = true;
                    return;
                }

                CanStart = false;
                CanStop = true;

                WebProxy proxy = null;
                if (_configModel.IsUseProxy)
                {
                    proxy = new WebProxy(_configModel.ProxyData.IP, _configModel.ProxyData.Port);
                }

                try
                {
                    _iStockRealTimeDealCrawler?.StartCrawler(proxy, _configModel.ClawlerConfigData.QueryInterval, monitorStocks);
                }
                catch (ClawlerConfigException clawlerConfigException)
                {
                    Messenger.Default.Send(new StatusBarArgs(true, clawlerConfigException.Message), StatusBarToken.UpdateStatus);
                    await Task.Run(() => _iStockRealTimeDealCrawler?.StopCrawler());
                    this.CanStart = true;
                    this.CanStop = false;
                    return;
                }
                catch (Exception ex)
                {
                    LogBuilder.Logger.Error("start crawler error.", ex);
                }

                if (_configModel.IsUseBigOrderStrategy)
                {
                    try
                    {
                        var stockAnalysisService = ServiceManager<StockTransactionModel>.Instance.GetStockAnalysisService();
                        _stockAnalysisService = stockAnalysisService;
                        _stockAnalysisService.StockAnalysisResultUpdatedEvent += StockAnalysisService_StockAnalysisResultUpdatedEvent;
                        _stockAnalysisService.RemainderCountUpdatedEvent += _stockAnalysisService_RemainderCountUpdatedEvent;
                        _stockAnalysisService.StartAnalysis(new BigDealAnalysisStrategy(), _cacheBigDealAnalysisStrategy);
                    }
                    catch (StrategyConfigException strategyConfigException)
                    {
                        _stockAnalysisService.StockAnalysisResultUpdatedEvent -= StockAnalysisService_StockAnalysisResultUpdatedEvent;
                        _stockAnalysisService.RemainderCountUpdatedEvent -= _stockAnalysisService_RemainderCountUpdatedEvent;
                        _stockAnalysisService.StopAnalysis();
                        Messenger.Default.Send(new StatusBarArgs(true, strategyConfigException.Message), StatusBarToken.UpdateStatus);
                        await Task.Run(() => _iStockRealTimeDealCrawler?.StopCrawler());
                        this.CanStart = true;
                        this.CanStop = false;
                        return;
                    }
                    catch (Exception ex)
                    {
                        LogBuilder.Logger.Error("start moniter BigOrderStrategy error.", ex);
                    }
                }

                if (_configModel.IsUseRapidRiseStrategy)
                {
                    try
                    {
                        var quickAnalysisService = ServiceManager<StockTransactionModelExtern>.Instance.GetStockAnalysisService();
                        _quickAnalysisService = quickAnalysisService;
                        _quickAnalysisService.StockAnalysisResultUpdatedEvent += _quickAnalysisService_StockAnalysisResultUpdatedEvent;
                        _quickAnalysisService.RemainderCountUpdatedEvent += _quickAnalysisService_RemainderCountUpdatedEvent;
                        _quickAnalysisService.StartAnalysis(new QuickUpAnalysisStrategy(), _cacheQuickUpAnalysisStrategy);
                    }
                    catch (StrategyConfigException strategyConfigException)
                    {
                        _quickAnalysisService.StockAnalysisResultUpdatedEvent -= _quickAnalysisService_StockAnalysisResultUpdatedEvent;
                        _quickAnalysisService.RemainderCountUpdatedEvent -= _quickAnalysisService_RemainderCountUpdatedEvent;
                        _quickAnalysisService.StopAnalysis();
                        Messenger.Default.Send(new StatusBarArgs(true, strategyConfigException.Message), StatusBarToken.UpdateStatus);
                        await Task.Run(() => _iStockRealTimeDealCrawler?.StopCrawler());
                        this.CanStart = true;
                        this.CanStop = false;
                        return;
                    }
                    catch (Exception ex)
                    {
                        LogBuilder.Logger.Error("start moniter RapidRiseStrategy error.", ex);
                    }
                }
                MonitoringStockCount = monitorStocks.Count;
                Messenger.Default.Send(new StatusBarArgs(true, ResourceHelper.FindKey("StatusBar_Message_Monitoring")), StatusBarToken.UpdateStatus);
            }
            catch (Exception ex)
            {
                CanStart = true;
                CanStop = false;
                Messenger.Default.Send(new StatusBarArgs(true, string.Format(ResourceHelper.FindKey("StatusBar_Message_StartMonitorFailed"), ex.Message)), StatusBarToken.UpdateStatus);
                LogBuilder.Logger.Error("开始监控 - 失败", ex);
            }
        }

        /// <summary>
        /// 停止监控
        /// </summary>
        private async void OnStop()
        {
            CanStop = false;
            Messenger.Default.Send(new StatusBarArgs(true, ResourceHelper.FindKey("StatusBar_Message_StoppingMonitor")), StatusBarToken.UpdateStatus);

            await Task.Run(() =>
            {
                _iStockRealTimeDealCrawler?.StopCrawler();
                if (_stockAnalysisService != null)
                {
                    _stockAnalysisService.StockAnalysisResultUpdatedEvent -= StockAnalysisService_StockAnalysisResultUpdatedEvent;
                    _stockAnalysisService.RemainderCountUpdatedEvent -= _stockAnalysisService_RemainderCountUpdatedEvent;
                    _stockAnalysisService.StopAnalysis();
                }

                if (_quickAnalysisService != null)
                {
                    _quickAnalysisService.StockAnalysisResultUpdatedEvent -= _quickAnalysisService_StockAnalysisResultUpdatedEvent;
                    _quickAnalysisService.RemainderCountUpdatedEvent -= _quickAnalysisService_RemainderCountUpdatedEvent;
                    _quickAnalysisService.StopAnalysis();
                }

                _cacheBigDealAnalysisStrategy = new ConcurrentQueue<StockTransactionModel>();
                _cacheQuickUpAnalysisStrategy = new ConcurrentQueue<StockTransactionModelExtern>();
            });

            CanStart = true;
            Messenger.Default.Send(new StatusBarArgs(false, string.Empty), StatusBarToken.UpdateStatus);
        }
        #endregion

        #region private methods

        private void InitData()
        {
            CanStart = true;
            CanStop = false;

            //获取配置信息
            _configModel = ConfigJsonHelper.GetConfigModel();
        }

        /// <summary>
        /// 订阅
        /// </summary>
        private void Subscribe()
        {
            Messenger.Default.Register<string>(this, MonitorOptions.StopMonitor, StopMonitor);
        }

        /// <summary>
        /// 停止监控
        /// </summary>
        /// <param name="message"></param>
        private void StopMonitor(string message)
        {
            if (CanStop)
                OnStop();
        }

        /// <summary>
        /// 筛选股票
        /// </summary>
        /// <returns></returns>
        private List<string> GetMonitorStocks()
        {
            List<string> result = new List<string>();
            //检查个股基本数据
            if (!File.Exists(LocalDataPath.LocalStockBasicDataPath) || !File.Exists(LocalDataPath.LocalStockBasicDataSyncTime))
            {
                Messenger.Default.Send(new StatusBarArgs(true, ResourceHelper.FindKey("StatusBar_Message_NotSyncStockBasicData")), StatusBarToken.UpdateStatus);
                return null;
            }

            //保证个股基本数据为当天的 (因为开盘价/昨日收盘价等信息都已当天为准)
            var jsonString = File.ReadAllText(LocalDataPath.LocalStockBasicDataSyncTime);
            var localStockBasicDataModel = JsonHelper.DeserializeJsonToObject<LocalStockBasicDataModel>(jsonString);
            if (DateTime.Parse(localStockBasicDataModel.SyncTime) <= DateTime.Parse($"{_stockMaketOpenTime}"))
            {
                Messenger.Default.Send(new StatusBarArgs(true, ResourceHelper.FindKey("StatusBar_Message_NotSyncStockBasicData")), StatusBarToken.UpdateStatus);
                return null;
            }

            //检查题材与个股关系数据
            if (!File.Exists(LocalDataPath.TopicStockLinkDataPath) || !File.Exists(LocalDataPath.TopicStockLinkDataSyncTime))
            {
                Messenger.Default.Send(new StatusBarArgs(true, ResourceHelper.FindKey("StatusBar_Message_NotSyncStockTopicLinkData")), StatusBarToken.UpdateStatus);
                return null;
            }

            //加载个股基本数据
            bool loadStockBasicDataSuc = CommonStockDataManager.Instance.LoadStockDetailModel();
            if (!loadStockBasicDataSuc)
            {
                Messenger.Default.Send(new StatusBarArgs(true, ResourceHelper.FindKey("StatusBar_Message_NotSyncStockBasicData")), StatusBarToken.UpdateStatus);
                return null;
            }

            //加载题材与个股关系数据
            bool loadStorageSectionsSuc = CommonStockDataManager.Instance.LoadStorageSections();
            if (!loadStorageSectionsSuc)
            {
                Messenger.Default.Send(new StatusBarArgs(true, ResourceHelper.FindKey("StatusBar_Message_NotSyncStockTopicLinkData")), StatusBarToken.UpdateStatus);
                return null;
            }

            //条件过滤筛选
            if (_configModel.IsFromConditionFilter)
            {
                List<string> conditionStocks = GetConditionStocks(CommonStockDataManager.Instance.GetStockDetailModel(false));
                result.AddRange(conditionStocks);
            }

            //需要满足股票代码正则
            Regex regex = new Regex(@"^(((002|000|300|600)[\d]{3})|60[\d]{4})$");

            //来自于本地文件筛选
            if (_configModel.IsFromLocalFile)
            {
                if (!File.Exists(_configModel.MonitorFilePath))
                {
                    Messenger.Default.Send(new StatusBarArgs(true, ResourceHelper.FindKey("StatusBar_ConfigError_FromFilePathIllegal")), StatusBarToken.UpdateStatus);
                    return null;
                }
                var stockLines = File.ReadAllLines(_configModel.MonitorFilePath).Where(x => !string.IsNullOrEmpty(x)).ToList();

                stockLines.ForEach(x =>
                {
                    if (regex.IsMatch(x.Trim()))
                        result.Add(x.Trim());
                });
            }

            //过滤不想监控的
            if (_configModel.IsExceptMonitorRange)
            {
                if (!File.Exists(_configModel.ExceptMonitorFilePath))
                {
                    Messenger.Default.Send(new StatusBarArgs(true, ResourceHelper.FindKey("StatusBar_ConfigError_ExceptFilePathIllegal")), StatusBarToken.UpdateStatus);
                    return null;
                }
                var stockLines = File.ReadAllLines(_configModel.ExceptMonitorFilePath).Where(x => !string.IsNullOrEmpty(x)).ToList();
                result = result.Except(stockLines.Select(s => s.Trim())).ToList();
            }

            return result.Distinct().ToList();
        }

        /// <summary>
        /// 根据条件筛选
        /// </summary>
        /// <param name="stockDetailModels"></param>
        /// <param name="storageStockSectionModels"></param>
        /// <returns></returns>
        private List<string> GetConditionStocks(List<StockDetailModel> stockDetailModels)
        {
            List<string> resultCodes = new List<string>();
            var models = stockDetailModels.ToList();
            var conditionData = _configModel.FromConditionFilterData;

            if (conditionData.IsFilterPrice)
            {
                models = models.Where(x => x.Price >= conditionData.PriceSmall && x.Price <= conditionData.PriceMax).ToList();
            }

            if (conditionData.IsFilterFlowValue)
            {
                models = models.Where(x => x.FlowlValue >= conditionData.FlowValueSmall * 100000000 && x.FlowlValue <= conditionData.FlowValueMax * 100000000).ToList();
            }

            if (conditionData.IsFilterTopic)
            {
                var filterTopics = conditionData.FilterTopicText?.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                if (filterTopics != null && filterTopics.Any())
                {

                    var codes = models.Select(x => x.Code).ToList();
                    Dictionary<string, string> tempDic = new Dictionary<string, string>();
                    foreach (var item in codes)
                    {
                        tempDic.Add(item, CommonStockDataManager.Instance.GetStorageSectionsStr(item));
                    }

                    foreach (var item in tempDic)
                    {
                        //逻辑为'与'
                        if (conditionData.IsLogicAnd)
                        {
                            bool isAllRight = true;
                            foreach (var filterTopic in filterTopics)
                            {
                                if (!item.Value.ToLower().Contains(filterTopic.ToLower()))
                                {
                                    isAllRight = false;
                                    break;
                                }
                            }

                            if (isAllRight) resultCodes.Add(item.Key);
                        }
                        //逻辑为'或'
                        else
                        {
                            bool isAllRight = false;
                            foreach (var filterTopic in filterTopics)
                            {
                                if (item.Value.ToLower().Contains(filterTopic.ToLower()))
                                {
                                    isAllRight = true;
                                    break;
                                }
                            }

                            if (isAllRight) resultCodes.Add(item.Key);
                        }
                    }

                    models = models.Where(x => resultCodes.Contains(x.Code)).ToList();
                }
            }
            else
            {
                resultCodes.AddRange(models.Select(x => x.Code));
            }
            return resultCodes;
        }


        /// <summary>
        /// 检查配置
        /// </summary>
        /// <returns></returns>
        private bool CheckConfig()
        {
            //检查代理
            var checkResult = ProxyHelper.CheckProxyUsefull(_configModel.ProxyData.IP, _configModel.ProxyData.Port, 10000);
            if (!checkResult)
            {
                Messenger.Default.Send(new StatusBarArgs(true, ResourceHelper.FindKey("StatusBar_ConfigError_ProxyUnavailable")), StatusBarToken.UpdateStatus);
                return false;
            }

            if (!_configModel.IsFromConditionFilter && !_configModel.IsFromLocalFile)
            {
                Messenger.Default.Send(new StatusBarArgs(true, ResourceHelper.FindKey("StatusBar_ConfigError_NotChooseMonitorRange")), StatusBarToken.UpdateStatus);
                return false;
            }

            if (_configModel.IsFromLocalFile && !File.Exists(_configModel.MonitorFilePath))
            {
                Messenger.Default.Send(new StatusBarArgs(true, ResourceHelper.FindKey("StatusBar_ConfigError_FromFilePathIllegal")), StatusBarToken.UpdateStatus);
                return false;
            }

            if (_configModel.IsExceptMonitorRange && !File.Exists(_configModel.ExceptMonitorFilePath))
            {
                Messenger.Default.Send(new StatusBarArgs(true, ResourceHelper.FindKey("StatusBar_ConfigError_ExceptFilePathIllegal")), StatusBarToken.UpdateStatus);
                return false;
            }

            if (!_configModel.IsUseBigOrderStrategy && !_configModel.IsUseRapidRiseStrategy)
            {
                Messenger.Default.Send(new StatusBarArgs(true, ResourceHelper.FindKey("StatusBar_ConfigError_NotChooseStrategy")), StatusBarToken.UpdateStatus);
                return false;
            }

            return true; ;
        }

        /// <summary>
        /// 数据来源
        /// </summary>
        /// <param name="args"></param>
        private void StockRealTimeDealCrawler_StockRealTimeDealUpdatedEven(StockRealTimeDealUpdatedEventArgs args)
        {
            args.AddedItems.ForEach(x =>
            {
                _cacheBigDealAnalysisStrategy.Enqueue(x);
                var temp = new StockTransactionModelExtern(x);
                _cacheQuickUpAnalysisStrategy.Enqueue(temp);
            });

            DispatcherHelper.RunAsync(new Action(() =>
            {
                args.AddedItems.ForEach(x =>
                {
                    _count++;
                    this.TotalCrawledCount = _count;
                });
            }));
        }

        /// <summary>
        /// 快速上涨
        /// </summary>
        /// <param name="args"></param>
        private void _quickAnalysisService_StockAnalysisResultUpdatedEvent(StockAnalysisResultUpdatedEventArgs<StockTransactionModelExtern> args)
        {
            DispatcherHelper.RunAsync(new Action(() =>
            {
                args.AnalysisResults.ForEach(x =>
                {
                    var model = CommonStockDataManager.Instance.GetStockDetailModel(x.Code);
                    if (model != null)
                        x.Name = model.Name;
                    var bks = CommonStockDataManager.Instance.GetStorageSectionsStr(x.Code);
                    if (!string.IsNullOrEmpty(bks))
                        x.Topic = bks;
                    //contentListBox.Items.Add($"[快速] - {x.ToString()}");
                    var temp = new StockModel(x);
                    StockModels.Insert(0, temp);
                });
            }));
        }

        /// <summary>
        /// 大单
        /// </summary>
        /// <param name="args"></param>
        private void StockAnalysisService_StockAnalysisResultUpdatedEvent(StockAnalysisResultUpdatedEventArgs<StockTransactionModel> args)
        {
            DispatcherHelper.RunAsync(new Action(() =>
            {
                args.AnalysisResults.ForEach(x =>
                {
                    var model = CommonStockDataManager.Instance.GetStockDetailModel(x.Code);
                    if (model != null)
                        x.Name = model.Name;
                    var bks = CommonStockDataManager.Instance.GetStorageSectionsStr(x.Code);
                    if (!string.IsNullOrEmpty(bks))
                        x.Topic = bks;
                    //contentListBox.Items.Add($"[大单] - {x.ToString()}");
                    var temp = new StockModel(x);
                    StockModels.Insert(0, temp);
                });
            }));
        }

        private void _stockAnalysisService_RemainderCountUpdatedEvent(object sender, int e)
        {
            BigDealRemainCount = e;
        }

        private void _quickAnalysisService_RemainderCountUpdatedEvent(object sender, int e)
        {
            QuickRemainCount = e;
        }

        private void ChangePageTo(int index)
        {
            if (index <= 0)
                return;
            PageStockModels.Clear();
            var temp = new List<StockModel>(StockModels.Skip((CurrentPageIndex - 1) * PageSize).Take(PageSize));
            temp.ToList().ForEach(m => PageStockModels.Add(m));
        }

        private void _stockModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ChangePageTo(CurrentPageIndex);
            //if (PageStockModels.Count < PageSize && e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            //{
            //    PageStockModels.Insert(0, StockModels.FirstOrDefault());
            //}
        }
        #endregion
    }
}
