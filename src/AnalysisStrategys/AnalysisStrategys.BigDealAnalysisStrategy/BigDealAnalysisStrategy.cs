using StockAnalysisInterfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StockAnalysisInterfaces.EventArgs;
using StockHelpers;
using Structures.Stocks;
using CommonHelpers.Config;
using StockAnalysisInterfaces.Exceptions;
using CommonHelpers;

namespace AnalysisStrategys.BigDealAnalysisStrategy
{
    public class BigDealAnalysisStrategy : IAnalysisStrategy<StockTransactionModel>
    {
        #region fields
        /// <summary>
        /// 每个标识分组的股票个数
        /// </summary>
        private volatile bool _isStarted = false;
        private volatile bool _isStoped = false;

        private ConfigJsonModel _configModel;
        private int _itemWorkCount = 300;
        private int _forwardSeconds = 60;
        private int _afterSeconds = 30;
        private double _bigDealAmountThreshold = 200000;
        private int _bigDealCountThreshold = 2;

        private ConcurrentQueue<StockTransactionModel> _cacheStockTransactionModels;
        /// <summary>
        /// key:标识
        /// value:{key:股票代码,value:个股成交Model集合}
        /// </summary>
        private ConcurrentDictionary<int, ConcurrentDictionary<string, ConcurrentQueue<StockTransactionModel>>> _modelsCaches = new ConcurrentDictionary<int, ConcurrentDictionary<string, ConcurrentQueue<StockTransactionModel>>>();

        private ConcurrentDictionary<StockTransactionModel, Tuple<int, StockTransactionModelStatus>> _historyModels = new ConcurrentDictionary<StockTransactionModel, Tuple<int, StockTransactionModelStatus>>();

        /// <summary>
        /// 已经触发过事件的集合
        /// </summary>
        private ConcurrentQueue<StockTransactionModel> _invokedModels = new ConcurrentQueue<StockTransactionModel>();

        private ConcurrentDictionary<int, Tuple<StockTransactionModel, DateTime>> _removeConditions = new ConcurrentDictionary<int, Tuple<StockTransactionModel, DateTime>>();
        #endregion

        public BigDealAnalysisStrategy()
        {
        }


        #region events
        /// <summary>
        /// 分析产生的结果引发事件
        /// </summary>
        public event StockAnalysisResultUpdatedEventHandler<StockTransactionModel> AnalysisResultUpdatedEvent;
        /// <summary>
        /// 待处理的数据量
        /// </summary>
        public event EventHandler<int> RemainderCountUpdatedEvent;

        #endregion

        /// <summary>
        /// 开始分析
        /// </summary>
        /// <param name="stockBases"></param>
        public void Start(IEnumerable<StockTransactionModel> stockBases)
        {
            if (_isStarted)
                return;
            _isStarted = true;
            _isStoped = false;
            _cacheStockTransactionModels = stockBases as ConcurrentQueue<StockTransactionModel>;
            if (_cacheStockTransactionModels == null)
                throw new ArgumentException($"'stockBases' type must be ConcurrentQueue<StockTransactionModel>.");

            _configModel = ConfigJsonHelper.GetConfigModel();

            CheckConfig();

            _itemWorkCount = _configModel.BigDealStrategyConfigData.StockMaxCountEachGroup;
            int threadCount = _configModel.BigDealStrategyConfigData.ThreadCount;
            this._forwardSeconds = _configModel.BigDealStrategyConfigData.ForwardSeconds;
            this._afterSeconds = _configModel.BigDealStrategyConfigData.AfterSeconds;
            this._bigDealAmountThreshold = _configModel.BigDealStrategyConfigData.BigDealAmountThreshold;
            this._bigDealCountThreshold = (int)_configModel.BigDealStrategyConfigData.BigDealCountThreshold;

            //使用三个线程来完成大单策略分析
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
                            int count = _cacheStockTransactionModels.Count;
                            if (remainCount != count)
                            {
                                remainCount = count;
                                RemainderCountUpdatedEvent.Invoke(this, count);
                            }

                            bool isDequeueSuccess = _cacheStockTransactionModels.TryDequeue(out var model);
                            if (isDequeueSuccess)
                            {
                                if (!_modelsCaches.Any())
                                {
                                    _modelsCaches.TryAdd(0, new ConcurrentDictionary<string, ConcurrentQueue<StockTransactionModel>>());
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
                                        _modelsCaches[i].TryAdd(model.Code, new ConcurrentQueue<StockTransactionModel>(new List<StockTransactionModel> { model }));
                                        break;
                                    }

                                    //如果字典容量超过所设定的容量限制
                                    if (!_modelsCaches.ContainsKey(i + 1))
                                    {
                                        _modelsCaches.TryAdd(i + 1, new ConcurrentDictionary<string, ConcurrentQueue<StockTransactionModel>>());
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

       

        /// <summary>
        /// 停止分析
        /// </summary>
        public void Stop()
        {
            if (!_isStarted)
                return;
            _isStarted = false;
            _isStoped = true;

            _cacheStockTransactionModels = null;
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
        /// 策略分析过程
        /// </summary>
        /// <param name="groupDic"></param>
        /// <param name="cpuCore"></param>
        private void StockGroupWork(ConcurrentDictionary<string, ConcurrentQueue<StockTransactionModel>> groupDic, int cpuCore)
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
                        var allModels = groupDic.SelectMany(x => x.Value).OrderBy(x => x.Time);
                        if (!allModels.Any())
                        {
                            Thread.Sleep(1);
                            continue;
                        }
                        var last = allModels.LastOrDefault();
                        //2分钟没有更新数据则清空当前缓存
                        var findRemoveLast = _removeConditions.LastOrDefault(x => x.Key == threadId);
                        if (!_removeConditions.Any(x => x.Key == threadId))
                            _removeConditions.TryAdd(threadId, new Tuple<StockTransactionModel, DateTime>(last, DateTime.Now));
                        else
                        {
                            if (last != findRemoveLast.Value.Item1)
                                _removeConditions[threadId] = new Tuple<StockTransactionModel, DateTime>(last, DateTime.Now);
                            else if ((DateTime.Now - findRemoveLast.Value.Item2).TotalMinutes >= 2)
                            {
                                foreach (var item in groupDic)
                                {
                                    item.Value.ToList().ForEach(x => item.Value.TryDequeue(out x));
                                }
                            }
                        }

                        //筛选时间在09:30:00 - 15:02:00，成交类型为-买入，股价状态为-非下跌，成交金额-大于20万的成交单
                        string startTime = "09:30:00";
                        string endTime = "15:02:00";
                        var reulst = allModels
                                    .Where(model => model.DealType == BuyOrSale.Buy && model.Status != StockStatus.Down && (model.Price * model.DealHands * 100) >= this._bigDealAmountThreshold)
                                    .Where(model => model.Time.CompareTo(startTime) > 0 && model.Time.CompareTo(endTime) < 0)
                                    .Except(_historyModels.Keys).ToList();

                        reulst.ForEach(x =>
                        {
                            x.Topic = CommonStockDataManager.Instance.GetStorageSectionsStr(x.Code);
                            var temp = CommonStockDataManager.Instance.GetStockDetailModel(x.Code);
                            if (temp != null)
                            {
                                x.CurrentPercent = Math.Round(100 * (x.Price - temp.YesterdayEndPrice) / temp.YesterdayEndPrice, 2, MidpointRounding.AwayFromZero);
                                x.Name = temp.Name;

                            }
                            _historyModels.TryAdd(x, new Tuple<int, StockTransactionModelStatus>(threadId, new StockTransactionModelStatus()));
                        });

                        var list = new List<KeyValuePair<StockTransactionModel, Tuple<int, StockTransactionModelStatus>>>();
                        //获取当前线程未检查过的数据
                        var findNotChecked = _historyModels.Where(x => x.Key != null && x.Value.Item1 == threadId && !x.Value.Item2.IsChecked).ToList();
                        findNotChecked.ForEach(x =>
                        {
                            var ranges = GetRangeStockTransactionModel(groupDic, x.Key, this._forwardSeconds, this._afterSeconds);
                            x.Value.Item2.IsChecked = true;
                            x.Value.Item2.IsNeedInvoke = IsAtConditions(ranges, x.Key);
                            list.Add(x);
                        });

                        var invokes = list.Where(x => x.Value.Item2.IsChecked && x.Value.Item2.IsNeedInvoke && !x.Value.Item2.IsInvoked)
                                          .Select(x => x.Key)
                                          .Except(_invokedModels).ToList();
                        if (invokes.Any())
                        {
                            invokes.ForEach(x => _invokedModels.Enqueue(x));

                            list.Where(x => x.Value.Item2.IsChecked && x.Value.Item2.IsNeedInvoke && !x.Value.Item2.IsInvoked).ToList()
                                .ForEach(x => x.Value.Item2.IsInvoked = true);

                            AnalysisResultUpdatedEvent?.Invoke(new StockAnalysisResultUpdatedEventArgs<StockTransactionModel>(invokes.OrderBy(x => x.Time).ToList()));
                        }
                    }
                    catch
                    {
                    }
                    Thread.Sleep(10);
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
        private List<StockTransactionModel> GetRangeStockTransactionModel(ConcurrentDictionary<string, ConcurrentQueue<StockTransactionModel>> groups, StockTransactionModel current, int forwardSec = 60, int afterSec = 30)
        {
            List<StockTransactionModel> result = new List<StockTransactionModel>();
            try
            {
                bool suc = groups.TryGetValue(current.Code, out var temp);
                if (suc)
                {
                    string currenttimteForward = DateTime.Parse(current.Time).AddSeconds(-forwardSec).ToString("HH:mm:ss");
                    string currenttimeAfter = DateTime.Parse(current.Time).AddSeconds(afterSec).ToString("HH:mm:ss");
                    var finds = temp.OrderBy(x => x.Time).Where(x => x.Time.CompareTo(currenttimteForward) >= 0 && x.Time.CompareTo(currenttimeAfter) <= 0).ToList();
                    result.AddRange(finds);

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
        private bool IsAtConditions(List<StockTransactionModel> ranges, StockTransactionModel currentModel)
        {
            if (ranges == null)
                return false;
            var usefulRanges = ranges.Where(x => x != null);

            var temp = CommonStockDataManager.Instance.GetStockDetailModel(currentModel.Code);
            if (temp == null)
                return false;

            //设置当前涨幅值
            currentModel.CurrentPercent = Math.Round(100 * (currentModel.Price - temp.YesterdayEndPrice) / temp.YesterdayEndPrice, 2, MidpointRounding.AwayFromZero);

            var upCount = usefulRanges.Where(model => model.Status != StockStatus.Down && (model.Price * model.DealHands * 100) >= this._bigDealAmountThreshold);

            //条件为: 在范围内存在状态为非下跌,且成交金额大于20w的单数至少3笔
            return upCount.Count() > this._bigDealCountThreshold;
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        private void ClearCaches()
        {
            _modelsCaches.Clear();
            _historyModels.Clear();
            _removeConditions.Clear();
            _invokedModels = new ConcurrentQueue<StockTransactionModel>();
        }

        /// <summary>
        /// 检查策略配置
        /// </summary>
        private void CheckConfig()
        {
            if (_configModel.BigDealStrategyConfigData.StockMaxCountEachGroup < 100 || _configModel.BigDealStrategyConfigData.StockMaxCountEachGroup > 500)
            {
                throw new StrategyConfigException($"{ResourceHelper.FindKey("Setting_StockMaxCountEachGroup")} must at the range [100-500].");
            }

            if (_configModel.BigDealStrategyConfigData.ThreadCount < 1 || _configModel.BigDealStrategyConfigData.ThreadCount > 10)
            {
                throw new StrategyConfigException($"{ResourceHelper.FindKey("Setting_ThreadCount")} must at the range [1-10].");
            }

            if (_configModel.BigDealStrategyConfigData.ForwardSeconds < 10 || _configModel.BigDealStrategyConfigData.ForwardSeconds > 60 * 5)
            {
                throw new StrategyConfigException($"{ResourceHelper.FindKey("Setting_ForwardSeconds")} must at the range [10s-300s].");
            }

            if (_configModel.BigDealStrategyConfigData.AfterSeconds < 0 || _configModel.BigDealStrategyConfigData.AfterSeconds > 60)
            {
                throw new StrategyConfigException($"{ResourceHelper.FindKey("Setting_AfterSeconds")} must at the range [0s-60s].");
            }

            if (_configModel.BigDealStrategyConfigData.BigDealAmountThreshold < 100000)
            {
                throw new StrategyConfigException($"{ResourceHelper.FindKey("Setting_BigDealSetting_BigDealAmountThreshold")} must bigger than 100000.");
            }

            if (_configModel.BigDealStrategyConfigData.BigDealCountThreshold < 0)
            {
                throw new StrategyConfigException($"{ResourceHelper.FindKey("Setting_BigDealSetting_BigDealCountThreshold")} must bigger than 0.");
            }
        }

        #endregion

    }
}
