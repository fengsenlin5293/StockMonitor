using CommonStockDatas;
using DataCenterServices;
using DataCenterServices.EventArgs;
using DataCenterServices.Interface;
using Structures;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AnalysisStrategys.BigDealAnalysisStrategy
{
    public class BigDealAnalysisStrategy : IAnalysisStrategy<StockTransactionModel>
    {
        private const int _itemWorkCount = 400;
        private ConcurrentQueue<StockTransactionModel> _caches;
        private ConcurrentDictionary<int, ConcurrentDictionary<string, ConcurrentQueue<StockTransactionModel>>> _modelsCaches = new ConcurrentDictionary<int, ConcurrentDictionary<string, ConcurrentQueue<StockTransactionModel>>>();
        private bool _isStarted = false;
        private volatile bool _isStoped = false;
        public BigDealAnalysisStrategy()
        {

        }

        #region events

        public event StockAnalysisResultUpdatedEventHandler<StockTransactionModel> AnalysisResultUpdatedEvent;

        #endregion

        public void Start(IEnumerable<StockTransactionModel> stockBases)
        {
            if (_isStarted)
                return;
            _isStarted = true;
            _isStoped = false;
            _caches = stockBases as ConcurrentQueue<StockTransactionModel>;
            if (_caches == null)
                throw new ArgumentException($"'stockBases' type must be ConcurrentQueue<StockTransactionModel>.");

            Task.Factory.StartNew(() =>
            {
                while (!_isStoped)
                {
                    try
                    {
                        StockTransactionModel model;
                        bool isDequeueSuccess = _caches.TryDequeue(out model);
                        if (isDequeueSuccess)
                        {
                            if (!_modelsCaches.Any())
                            {
                                _modelsCaches.TryAdd(0, new ConcurrentDictionary<string, ConcurrentQueue<StockTransactionModel>>());
                                ItemWork(_modelsCaches[0]);
                            }

                            for (int i = 0; ; i++)
                            {
                                if (_modelsCaches[i].ContainsKey(model.Code))
                                {
                                    _modelsCaches[i][model.Code].Enqueue(model);
                                    break;
                                }
                                else if (_modelsCaches[i].Count < _itemWorkCount)
                                {
                                    _modelsCaches[i].TryAdd(model.Code, new ConcurrentQueue<StockTransactionModel>(new List<StockTransactionModel> { model }));
                                    break;
                                }

                                if (!_modelsCaches.ContainsKey(i + 1))
                                {
                                    _modelsCaches.TryAdd(i + 1, new ConcurrentDictionary<string, ConcurrentQueue<StockTransactionModel>>());
                                    ItemWork(_modelsCaches[i + 1]);
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

        public void Stop()
        {
            if (!_isStarted)
                return;
            _isStarted = false;
            _isStoped = true;

            _caches = null;
        }


        private ConcurrentQueue<StockTransactionModel> _historyModels = new ConcurrentQueue<StockTransactionModel>();
        private ConcurrentQueue<StockTransactionModel> _invokedModels = new ConcurrentQueue<StockTransactionModel>();

        private void ItemWork(ConcurrentDictionary<string, ConcurrentQueue<StockTransactionModel>> item)
        {
            Task.Factory.StartNew(() =>
            {
                if (item == null)
                    throw new ArgumentNullException("item");
                while (!_isStoped)
                {
                    try
                    {
                        var reulst = item.SelectMany(x => x.Value)
                                    .Where(model => model.Status == StockStatus.Up && (model.Price * model.DealHands * 100) >= 500000)
                                    .Where(model =>
                                    {
                                        try
                                        {
                                            return DateTime.Parse(model.Time) > DateTime.Parse("09:30:00") &&
                                                   DateTime.Parse(model.Time) < DateTime.Parse("15:00:00");
                                        }
                                        catch (Exception)
                                        {

                                        }
                                        return false;
                                    })
                                    .Where(model =>
                                    {
                                        var temp = CommonStockDataManager.Instance.GetStockDetailModel(model.Code);
                                        if (temp != null && temp.FlowlValue <= 12000000000)
                                        {
                                            return true;
                                        }
                                        return false;
                                    })
                                    .Except(_historyModels).ToList();

                        var groups = item.SelectMany(x => x.Value)
                                    .Where(model =>
                                    {
                                        try
                                        {
                                            return DateTime.Parse(model.Time) > DateTime.Parse("09:30:00") &&
                                                   DateTime.Parse(model.Time) < DateTime.Parse("15:00:00");
                                        }
                                        catch (Exception)
                                        {

                                        }
                                        return false;
                                    })
                                    .OrderBy(x => x.Time)
                                    .GroupBy(x => GetGroupByString(x));

                        var tempsss = new List<StockTransactionModel>(_historyModels);
                        if (_invokedModels.Any())
                        {
                            //同一只股票选最新的异动来做筛选
                            tempsss.Clear();
                            var groups1 = _historyModels.OrderBy(x => x.Time).GroupBy(x => x.Code);
                            var groups2 = _invokedModels.OrderBy(x => x.Time).GroupBy(x => x.Code);
                            var sameKeys = groups1.Select(x => x.Key).Intersect(groups2.Select(x => x.Key)).ToList();
                            foreach (var groupItem in groups1)
                            {
                                //如果同一个股票没有触发事件的则取最后一个异动
                                if (!sameKeys.Contains(groupItem.Key))
                                {
                                    var last = groupItem.Where(x => x != null).ToList().OrderBy(x => x.Time).Last();
                                    if (last != null) tempsss.Add(last);
                                }
                                else
                                {
                                    var find = groups2.LastOrDefault(x => x.Key == groupItem.Key);
                                    var last = find.Where(x => x != null).ToList().OrderBy(x => x.Time).Last();
                                    if (last == null)
                                    {
                                        tempsss.AddRange(groupItem);
                                    }
                                    else
                                    {
                                        var findTemp = groupItem.Where(x => DateTime.Parse(x.Time) >= DateTime.Parse(last.Time));
                                        tempsss.AddRange(findTemp);
                                    }
                                }
                            }

                        }

                        var history = tempsss.Select(x => GetGroupByString(x)).Distinct().ToList();
                        var needInvokes = new List<string>();
                        var atConditions = history.Intersect(groups.Select(x => x.Key)).ToList();
                        if (atConditions.Any())
                        {
                            atConditions.ForEach(x =>
                            {
                                var flagItem = tempsss.FirstOrDefault(t => GetGroupByString(t).Equals(x));
                                var current = groups.FirstOrDefault(g => g.Key.Equals(x));
                                var nextStr = GetGroupByString(x, 1);
                                var next = groups.FirstOrDefault(g => g.Equals(nextStr));
                                if (IsAtConditions(current, flagItem) || IsAtConditions(next, flagItem))
                                {
                                    if (!_invokedModels.Select(temp => GetGroupByString(temp)).Contains(x))
                                        needInvokes.Add(x);
                                }
                            });
                        }
                        var invokes = _historyModels.Where(x => needInvokes.Contains(GetGroupByString(x))).ToList();
                        invokes.ForEach(x => _invokedModels.Enqueue(x));

                        reulst.ForEach(x => _historyModels.Enqueue(x));
                        if (invokes.Any())
                        {
                            AnalysisResultUpdatedEvent?.Invoke(new StockAnalysisResultUpdatedEventArgs<StockTransactionModel>(invokes));
                        }
                        //if (reulst.Any())
                        //{
                        //    AnalysisResultUpdatedEvent?.Invoke(new StockAnalysisResultUpdatedEventArgs<StockTransactionModel>(reulst));
                        //}
                    }
                    catch
                    {
                    }
                    Thread.Sleep(10);
                }
            }, TaskCreationOptions.LongRunning);
        }

        private bool IsAtConditions(IGrouping<string, StockTransactionModel> group, StockTransactionModel flagItem)
        {
            if (group == null)
                return false;
            var total = group.Where(x => x != null && x.Time.CompareTo(flagItem.Time) >= 0);
            //var upCount = total.Where(x => x.Status == StockStatus.Up);
            var upCount = total.Where(model => model.Status == StockStatus.Up && (model.Price * model.DealHands * 100) >= 500000);

            return upCount.Count() > 1;
        }

        private string GetGroupByString(StockTransactionModel model)
        {
            return $"{model.Code},[{DateTime.Parse(model.Time).ToString("HH:mm")}]";
        }

        private string GetGroupByString(string current, int nextCountMinutes)
        {
            var regex = new Regex("\\[(.*?)\\]");
            var match = regex.Match(current);
            var time = match.Value.Substring(1, 5);//[HH:mm]
            var newtime = DateTime.Parse(time).AddMinutes(nextCountMinutes);
            return current.Replace(match.Value, $"[{newtime.ToString("HH:mm")}]");
        }
    }
}
