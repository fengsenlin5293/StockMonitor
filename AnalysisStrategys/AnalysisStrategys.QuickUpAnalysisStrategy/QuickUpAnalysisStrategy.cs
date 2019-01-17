using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommonHelpers;
using CommonHelpers.Config;
using StockAnalysisInterfaces;
using StockAnalysisInterfaces.EventArgs;
using StockAnalysisInterfaces.Exceptions;
using StockHelpers;
using Structures.Stocks;


namespace AnalysisStrategys.QuickUpAnalysisStrategy
{
    public class QuickUpAnalysisStrategy : IAnalysisStrategy<StockTransactionModelExtern>
    {
        #region fields

        /// <summary>
        /// 每个标识分组的股票个数
        /// </summary>
        private volatile bool _isStarted = false;
        private volatile bool _isStoped = false;

        private ConfigJsonModel _configModel;
        private int _itemWorkCount = 200;
        private int _forwardSeconds = 60;
        private int _afterSeconds = 0;
        private double _dealAmountThreshold = 100000;
        private double _quickUpThreshold = 0.01;

        private ConcurrentQueue<StockTransactionModelExtern> _cacheStockTransactionModelExterns;
        /// <summary>
        /// 将所有的model分成若干组,每组最多 _itemWorkCount 个
        /// key:标识
        /// value:{key:股票代码,value:个股成交Model集合}
        /// </summary>
        private ConcurrentDictionary<int, ConcurrentDictionary<string, ConcurrentQueue<StockTransactionModelExtern>>> _modelsCaches = new ConcurrentDictionary<int, ConcurrentDictionary<string, ConcurrentQueue<StockTransactionModelExtern>>>();


        private ConcurrentDictionary<StockTransactionModelExtern, Tuple<int, StockTransactionModelStatus>> _historyModels = new ConcurrentDictionary<StockTransactionModelExtern, Tuple<int, StockTransactionModelStatus>>();
        /// <summary>
        /// 已经触发过事件的集合
        /// </summary>
        private ConcurrentQueue<StockTransactionModelExtern> _invokedModels = new ConcurrentQueue<StockTransactionModelExtern>();
        private ConcurrentDictionary<int, Tuple<StockTransactionModelExtern, DateTime>> _removeConditions = new ConcurrentDictionary<int, Tuple<StockTransactionModelExtern, DateTime>>();
        #endregion

        public QuickUpAnalysisStrategy()
        {

        }

        #region events

        public event StockAnalysisResultUpdatedEventHandler<StockTransactionModelExtern> AnalysisResultUpdatedEvent;
        public event EventHandler<int> RemainderCountUpdatedEvent;

        #endregion

        public void Start(IEnumerable<StockTransactionModelExtern> stockBases)
        {
            if (_isStarted)
                return;
            _isStarted = true;
            _isStoped = false;
            _cacheStockTransactionModelExterns = stockBases as ConcurrentQueue<StockTransactionModelExtern>;
            if (_cacheStockTransactionModelExterns == null)
                throw new ArgumentException($"'stockBases' type must be ConcurrentQueue<StockTransactionModelExtern>.");

            _configModel = ConfigJsonHelper.GetConfigModel();

            CheckConfig();

            _itemWorkCount = _configModel.QuickUpStrategyConfigData.StockMaxCountEachGroup;
            int threadCount = _configModel.QuickUpStrategyConfigData.ThreadCount;
            this._forwardSeconds = _configModel.QuickUpStrategyConfigData.ForwardSeconds;
            this._afterSeconds = _configModel.QuickUpStrategyConfigData.AfterSeconds;
            this._dealAmountThreshold = (int)_configModel.QuickUpStrategyConfigData.DealAmountThreshold;
            this._quickUpThreshold = _configModel.QuickUpStrategyConfigData.QuickUpThreshold / 100;

            for (int ntask = 1; ntask <= threadCount; ntask++)
            {
                Task.Factory.StartNew(() =>
                {
                    //System.Diagnostics.Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)ntask;
                    var remainCount = 0;
                    while (!_isStoped)
                    {
                        try
                        {
                            //剩余待处理的数量
                            int count = _cacheStockTransactionModelExterns.Count;
                            if (remainCount != count)
                            {
                                remainCount = count;
                                RemainderCountUpdatedEvent.Invoke(this, count);
                            }

                            bool isDequeueSuccess = _cacheStockTransactionModelExterns.TryDequeue(out var model);
                            if (isDequeueSuccess)
                            {
                                if (!_modelsCaches.Any())
                                {
                                    _modelsCaches.TryAdd(0, new ConcurrentDictionary<string, ConcurrentQueue<StockTransactionModelExtern>>());
                                    StockGroupWork(_modelsCaches[0], _modelsCaches.Count);
                                }

                                for (int i = 0; ; i++)
                                {
                                    //如果字典已有此个股
                                    if (_modelsCaches[i].ContainsKey(model.Code))
                                    {
                                        _modelsCaches[i][model.Code].Enqueue(model);
                                        break;
                                    }

                                    //如果未包含此个股,且字典容量小于所设定的容量限制
                                    if (_modelsCaches[i].Count < _itemWorkCount)
                                    {
                                        _modelsCaches[i].TryAdd(model.Code, new ConcurrentQueue<StockTransactionModelExtern>(new List<StockTransactionModelExtern> { model }));
                                        break;
                                    }

                                    //如果字典容量超过所设定的容量限制
                                    if (!_modelsCaches.ContainsKey(i + 1))
                                    {
                                        _modelsCaches.TryAdd(i + 1, new ConcurrentDictionary<string, ConcurrentQueue<StockTransactionModelExtern>>());
                                        StockGroupWork(_modelsCaches[i + 1], _modelsCaches.Count);
                                    }
                                    continue;
                                }
                            }
                            Thread.Sleep(1);
                        }
                        catch
                        {
                        }
                    }
                }, TaskCreationOptions.LongRunning);
            }

        }

        public void Stop()
        {
            if (!_isStarted)
                return;
            _isStarted = false;
            _isStoped = true;

            _cacheStockTransactionModelExterns = null;
            ClearCaches();
        }


        public class StockTransactionModelStatus
        {
            /// <summary>
            /// 是否检查过
            /// </summary>
            public bool IsChecked { get; set; }

            /// <summary>
            /// 是否需要触发事件
            /// </summary>
            public bool IsNeedInvoke { get; set; }

            /// <summary>
            /// 已经触发了事件
            /// </summary>
            public bool IsInvoked { get; set; }
        }

        #region private methods

        /// <summary>
        /// 清除缓存
        /// </summary>
        private void ClearCaches()
        {
            _modelsCaches.Clear();
            _historyModels.Clear();
            _removeConditions.Clear();
            _invokedModels = new ConcurrentQueue<StockTransactionModelExtern>();
        }


        /// <summary>
        /// 策略分析过程
        /// </summary>
        /// <param name="groupDic"></param>
        /// <param name="cpuCore"></param>
        private void StockGroupWork(ConcurrentDictionary<string, ConcurrentQueue<StockTransactionModelExtern>> groupDic, int cpuCore)
        {
            Task.Factory.StartNew(() =>
            {
                //System.Diagnostics.Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)cpuCore;
                if (groupDic == null)
                    throw new ArgumentNullException("groupDic");
                int threadId = Thread.CurrentThread.ManagedThreadId;
                while (!_isStoped)
                {
                    try
                    {
                        var tempModels = groupDic.Values.SelectMany(x => x).OrderBy(x => x.Time);
                        if (!tempModels.Any())
                        {
                            Thread.Sleep(1);
                            continue;
                        }
                        var allModels = tempModels.ToList();
                        var last = allModels.LastOrDefault();
                        //2分钟没有更新数据则清空当前缓存
                        var findRemoveLast = _removeConditions.LastOrDefault(x => x.Key == threadId);
                        if (!_removeConditions.Any(x => x.Key == threadId))
                            _removeConditions.TryAdd(threadId, new Tuple<StockTransactionModelExtern, DateTime>(last, DateTime.Now));
                        else
                        {
                            if (last != findRemoveLast.Value.Item1)
                                _removeConditions[threadId] = new Tuple<StockTransactionModelExtern, DateTime>(last, DateTime.Now);
                            else if ((DateTime.Now - findRemoveLast.Value.Item2).TotalMinutes >= 2)
                                foreach (var item in groupDic)
                                {
                                    item.Value.ToList().ForEach(x => item.Value.TryDequeue(out x));
                                }
                        }

                        //筛选时间在09:30:00 - 15:02:00，成交类型为-买入，成交金额-大于10万的成交单
                        string startTime = "09:30:00";
                        string endTime = "15:02:00";
                        var reulst = allModels
                                    .Where(model => model.DealType == BuyOrSale.Buy && (model.Price * model.DealHands * 100) >= this._dealAmountThreshold)
                                    .Where(model => model.Time.CompareTo(startTime) > 0 && model.Time.CompareTo(endTime) < 0)
                                    .Except(_historyModels.Keys).ToList();

                        reulst.ForEach(x =>
                        {
                            _historyModels.TryAdd(x, new Tuple<int, StockTransactionModelStatus>(threadId, new StockTransactionModelStatus()));
                        });

                        var list = new List<KeyValuePair<StockTransactionModelExtern, Tuple<int, StockTransactionModelStatus>>>();
                        //获取当前线程未检查过的数据
                        var findNotChecked = _historyModels.Where(x => x.Key != null && x.Value.Item1 == threadId && !x.Value.Item2.IsChecked).ToList();
                        findNotChecked.ForEach(x =>
                        {
                            var ranges = GetRangeStockTransactionModel(groupDic, x.Key, this._forwardSeconds, this._afterSeconds);
                            bool isNeedInvoke = IsAtConditions(ranges, x.Key);
                            x.Value.Item2.IsChecked = true;
                            x.Value.Item2.IsNeedInvoke = isNeedInvoke;
                            list.Add(x);
                        });

                        var invokes = list.Where(x => x.Value.Item2.IsChecked && x.Value.Item2.IsNeedInvoke && !x.Value.Item2.IsInvoked)
                                          .Select(x => x.Key)
                                          .Except(_invokedModels).ToList();

                        if (invokes.Any())
                        {
                            invokes.ForEach(x =>
                            {
                                x.Topic = CommonStockDataManager.Instance.GetStorageSectionsStr(x.Code);
                                x.Name = CommonStockDataManager.Instance.GetStockDetailModel(x.Code).Name;
                                _invokedModels.Enqueue(x);
                            });

                            list.Where(x => x.Value.Item2.IsChecked && x.Value.Item2.IsNeedInvoke && !x.Value.Item2.IsInvoked).ToList()
                                .ForEach(x => x.Value.Item2.IsInvoked = true);

                            AnalysisResultUpdatedEvent?.Invoke(new StockAnalysisResultUpdatedEventArgs<StockTransactionModelExtern>(invokes.OrderBy(x => x.Time).ToList()));
                        }
                    }
                    catch
                    {
                    }
                    Thread.Sleep(1);
                }
            }, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// 获取当前成交单的前一段时间内及之后一段时间内的数据
        /// </summary>
        /// <param name="groups"></param>
        /// <param name="current"></param>
        /// <param name="forwardSec">默认:60s</param>
        /// <param name="afterSec">默认:30s</param>
        /// <returns></returns>
        private List<StockTransactionModelExtern> GetRangeStockTransactionModel(ConcurrentDictionary<string, ConcurrentQueue<StockTransactionModelExtern>> groups, StockTransactionModelExtern current, int forwardSec = 60, int afterSec = 0)
        {
            List<StockTransactionModelExtern> result = new List<StockTransactionModelExtern>();
            try
            {
                bool suc = groups.TryGetValue(current.Code, out var temp);
                if (suc)
                {
                    string currenttimteForward = DateTime.Parse(current.Time).AddMinutes(-1).ToString("HH:mm:ss");
                    var findRanges = temp.OrderBy(x => x.Time).Where(x => x.Time.CompareTo(currenttimteForward) >= 0 && x.Time.CompareTo(current.Time) <= 0);
                    result.AddRange(findRanges);

                    //清除比当前个股前一分钟之前的缓存数据
                    var rmvs = temp.Where(x => x.Time.CompareTo(currenttimteForward) < 0).ToList();
                    rmvs.ForEach(x => temp.TryDequeue(out x));
                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        /// <summary>
        /// 检查当前成交单是否满足策略
        /// </summary>
        /// <param name="ranges"></param>
        /// <param name="currentModel"></param>
        /// <returns></returns>
        private bool IsAtConditions(List<StockTransactionModelExtern> ranges, StockTransactionModelExtern currentModel)
        {
            if (ranges == null)
                return false;
            var usefullRanges = ranges.Where(x => x != null);

            if (!usefullRanges.Any())
                return false;

            var maxPrice = currentModel.Price;
            if (usefullRanges.Any())
                maxPrice = usefullRanges.Max(x => x.Price);

            var minPrice = currentModel.Price;
            if (usefullRanges.Any())
                minPrice = usefullRanges.Min(x => x.Price);
            bool isRise = usefullRanges.FirstOrDefault(x => x.Price == maxPrice).Time.CompareTo(usefullRanges.FirstOrDefault(x => x.Price == minPrice).Time) > 0;

            var temp = CommonStockDataManager.Instance.GetStockDetailModel(currentModel.Code);
            if (temp == null)
                return false;
            //当前的涨幅
            currentModel.CurrentPercent = Math.Round(100 * (currentModel.Price - temp.YesterdayEndPrice) / temp.YesterdayEndPrice, 2, MidpointRounding.AwayFromZero);
            //快速上涨的幅度
            currentModel.QuickUpPercent = Math.Round(100 * (maxPrice - minPrice) / temp.YesterdayEndPrice, 2, MidpointRounding.AwayFromZero);

            //条件为: 在范围内为上涨趋势，并且上涨幅度大于1%
            bool result = isRise && (maxPrice - minPrice) / temp.YesterdayEndPrice >= this._quickUpThreshold;

            return result;
        }

        /// <summary>
        /// 检查策略配置
        /// </summary>
        private void CheckConfig()
        {
            if (_configModel.QuickUpStrategyConfigData.StockMaxCountEachGroup < 100 || _configModel.QuickUpStrategyConfigData.StockMaxCountEachGroup > 500)
            {
                throw new StrategyConfigException($"{ResourceHelper.FindKey("Setting_StockMaxCountEachGroup")} must at the range [100-500].");
            }

            if (_configModel.QuickUpStrategyConfigData.ThreadCount < 1 || _configModel.QuickUpStrategyConfigData.ThreadCount > 10)
            {
                throw new StrategyConfigException($"{ResourceHelper.FindKey("Setting_ThreadCount")} must at the range [1-10].");
            }

            if (_configModel.QuickUpStrategyConfigData.ForwardSeconds < 10 || _configModel.QuickUpStrategyConfigData.ForwardSeconds > 60 * 5)
            {
                throw new StrategyConfigException($"{ResourceHelper.FindKey("Setting_ForwardSeconds")} must at the range [10s-300s].");
            }

            if (_configModel.QuickUpStrategyConfigData.AfterSeconds < 0 || _configModel.QuickUpStrategyConfigData.AfterSeconds > 60)
            {
                throw new StrategyConfigException($"{ResourceHelper.FindKey("Setting_AfterSeconds")} must at the range [0s-60s].");
            }

            if (_configModel.QuickUpStrategyConfigData.DealAmountThreshold < 50000)
            {
                throw new StrategyConfigException($"{ResourceHelper.FindKey("Setting_BigDealSetting_BigDealAmountThreshold")} must bigger than 50000.");
            }

            if (_configModel.QuickUpStrategyConfigData.QuickUpThreshold < 0.5 || _configModel.QuickUpStrategyConfigData.QuickUpThreshold > 100)
            {
                throw new StrategyConfigException($"{ResourceHelper.FindKey("Setting_BigDealSetting_BigDealCountThreshold")} must bigger than [0.5-100].");
            }
        }

        #endregion
    }
}
