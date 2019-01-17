using ClawlerInterfaces;
using ClawlerInterfaces.Events;
using ClawlerInterfaces.Exceptions;
using CommonHelpers;
using CommonHelpers.Config;
using DotNet.Log4Net;
using HttpHelpers;
using Interfaces.ClawlerInterfaces.EventArgs;
using JsonHelpers;
using StockHelpers;
using Structures.Dto;
using Structures.JsonModels;
using Structures.Stocks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
namespace Crawlers.StockRealTimeDealCrawler
{
    public class StockRealTimeDealCrawler : IStockRealTimeDealCrawler
    {
        #region fields
        private const int PAGESIZE_MIN = 20;
        private const int PAGESIZE_MAX = 14400;

        private volatile bool _isStarted = false;
        private volatile bool _isStoped = false;

        private string _url;
        private string _token;

        //每次请求的数据量,默认每次请求最新的20条数据
        private int _pageSize;
        private int _currentPage = 1;
        private string _orderAsc = "asc";
        private string _orderDesc = "desc";

        private int _taskWorkCodeCount = 100;
        private int _interval = 3000;


        private List<string> _monitorStocks;
        private WebProxy _webProxy;

        private ConfigJsonModel _configModel;

        private readonly List<WorkTaskModel> _workTasks = new List<WorkTaskModel>();
        private ConcurrentQueue<Tuple<string, string>> _responseJsonQueue = new ConcurrentQueue<Tuple<string, string>>();
        private Dictionary<string, List<string>> _datasDic = new Dictionary<string, List<string>>();
        #endregion

        #region events

        public event StockRealTimeDealUpdatedEventHandler StockRealTimeDealUpdatedEven;

        #endregion


        public StockRealTimeDealCrawler()
        {
            _monitorStocks = new List<string>();

            _token = "44c9d251add88e27b65ed86506f6e5da";
            _url = "http://mdfm.eastmoney.com/EM_UBG_MinuteApi/Js/Get?";
        }


        /// <summary>
        /// 开始爬取数据
        /// </summary>
        /// <param name="interval">单位:毫秒</param>
        public void StartCrawler(WebProxy webProxy, int interval, IEnumerable<string> stockCodes)
        {
            if (_isStarted)
                return;
            _isStarted = true;
            _isStoped = false;
            _interval = interval;
            _webProxy = webProxy;

            _configModel = ConfigJsonHelper.GetConfigModel();

            CheckConfig();

            _pageSize = GetPageSize();

            _taskWorkCodeCount = _configModel.ClawlerConfigData.CodeCountPerThread;
            _monitorStocks = new List<string>(stockCodes);

            //分配一个线程专门从缓存中取出数据及数据转换并通过事件通知订阅者
            Task.Factory.StartNew(() =>
            {
                while (!_isStoped)
                {
                    try
                    {
                        Tuple<string, string> tuple;
                        var dequeueSuccesful = _responseJsonQueue.TryDequeue(out tuple);
                        if (dequeueSuccesful)
                        {
                            var queryStockTransactionResponseModel = JsonHelper.DeserializeJsonToObject<QueryStockTransactionResponseModel>(tuple.Item2);
                            var addedStr = new List<string>();
                            if (_datasDic.ContainsKey(tuple.Item1))
                            {
                                addedStr = queryStockTransactionResponseModel.Value.Datas.Except(_datasDic[tuple.Item1]).ToList();
                                _datasDic[tuple.Item1].AddRange(addedStr);
                            }
                            else
                            {
                                addedStr = new List<string>(queryStockTransactionResponseModel.Value.Datas);
                                _datasDic.Add(tuple.Item1, addedStr);
                            }

                            if (addedStr.Any())
                            {
                                List<StockTransactionModel> addedModels = DtoHelper.GetStockTransactionModels(addedStr);
                                addedModels.ForEach(x => x.Code = tuple.Item1);
                                StockRealTimeDealUpdatedEven?.Invoke(new StockRealTimeDealUpdatedEventArgs(addedModels));
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                    Thread.Sleep(1);
                }

            }, TaskCreationOptions.LongRunning);


            var count = _monitorStocks.Count % _taskWorkCodeCount == 0 ? _monitorStocks.Count / _taskWorkCodeCount : _monitorStocks.Count / _taskWorkCodeCount + 1;
            for (int i = 1; i <= count; i++)
            {
                var codes = _monitorStocks.Take(i * _taskWorkCodeCount).Skip((i - 1) * _taskWorkCodeCount);
                WorkTaskModel model = new WorkTaskModel();
                model.StockCodes = new List<string>(codes);
                model.Cts = new CancellationTokenSource();

                //System.Diagnostics.Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)0X007F;
                //指定线程在哪个cpu上运行,由于此程序的cpu占用率过高,想通过这种方式降低cpu
                //此方式确实可以降低cpu,但是数据处理速度慢了很多,所以还是放弃这种方式
                int userCount = 4;
                int core = i % userCount == 0 ? 1 + i % userCount : i % userCount;

                model.WorkTask = new Task(() => { DoCrawler(model.StockCodes, model.Cts.Token, core); }, model.Cts.Token, TaskCreationOptions.LongRunning);
                _workTasks.Add(model);
            }

            _workTasks.ForEach(work =>
            {
                work.WorkTask.Start();
            });
        }

        private int GetPageSize()
        {
            var pageSize = _configModel.ClawlerConfigData.QueryCountPerTime;
            if (pageSize < PAGESIZE_MIN)
            {
                pageSize = PAGESIZE_MIN;
            }
            else if (pageSize > PAGESIZE_MAX)
            {
                pageSize = PAGESIZE_MAX;
            }
            return pageSize;
        }


        /// <summary>
        /// 停止
        /// </summary>
        public void StopCrawler()
        {
            if (!_isStarted)
                return;
            _isStarted = false;
            _isStoped = true;
            var tasks = _workTasks.Select(w => w.WorkTask).ToList();
            _workTasks.ForEach(work =>
            {
                work.Cts?.Cancel();
                work.Cts = null;
                work.WorkTask = null;
            });

            //等待所有任务都已取消
            var cts = new CancellationTokenSource();
            var task = Task.Run(() => { while (tasks.Any(t => !t.IsCompleted)) { cts.Token.ThrowIfCancellationRequested(); } }, cts.Token);
            bool isCompleted = task.Wait(TimeSpan.FromMilliseconds(_interval * 1.5));
            cts.Cancel();

            _responseJsonQueue = new ConcurrentQueue<Tuple<string, string>>();
            _datasDic = new Dictionary<string, List<string>>();

            _workTasks.Clear();
        }

        #region privates methods

        private void DoCrawler(List<string> stockCodes, CancellationToken token, int cpuCore)
        {
            try
            {
                var headers = new WebHeaderCollection();
                headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9");

                //get json string
                var regex = new Regex("[{](.*?)[}]{1,}");

                //System.Diagnostics.Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)cpuCore;
                //System.Diagnostics.Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)0X007F;

                while (true)
                {
                    if (token == null)
                        break;

                    stockCodes.ForEach(code =>
                    {
                        token.ThrowIfCancellationRequested();
                        string url = GetUrl(_token, _pageSize, _currentPage, code, _orderDesc);
                        var response = HttpHelper.CreateGetHttpResponse(url, 60000, new CustomerHeader(), headers, _webProxy);
                        var responseString = HttpHelper.GetResponseString(response);
                        var match = regex.Match(responseString);
                        _responseJsonQueue.Enqueue(new Tuple<string, string>(code, match.Value));

                        Thread.Sleep(1);
                    });
                    Thread.Sleep(_interval);
                }
            }
            catch (OperationCanceledException ex)
            {
                LogBuilder.Logger.Info("Crawler task canceled.");
            }
            catch (Exception ex)
            {
                LogBuilder.Logger.Error("DoCrawler Error.", ex);
            }
        }

        private string GetUrl(string token, int pageSize, int currentPage, string code, string order)
        {
            int market = (int)StockHelper.GetMarket(code);
            var randomNumber = new Random().Next(11111, 99999);
            var randomNumber2 = new Random().Next(100000, 999999);
            string parameters = $"dtype=all&token={token}&rows={pageSize}&cb=jQuery17205063938445283711_15374580{randomNumber}&page={currentPage}&id={code}{market}&gtvolume=&sort={order}&_=1537458{randomNumber2}";
            return _url + parameters;
        }


        /// <summary>
        /// 检查策略配置
        /// </summary>
        private void CheckConfig()
        {
            if (_configModel.ClawlerConfigData.CodeCountPerThread < 100 || _configModel.ClawlerConfigData.CodeCountPerThread > 300)
            {
                throw new ClawlerConfigException($"{ResourceHelper.FindKey("Setting_CrawlerSetting_CodeCountPerThread")} must at the range [100-300].");
            }

            if (_configModel.ClawlerConfigData.QueryCountPerTime < PAGESIZE_MIN || _configModel.ClawlerConfigData.QueryCountPerTime > PAGESIZE_MAX)
            {
                throw new ClawlerConfigException($"{ResourceHelper.FindKey("Setting_CrawlerSetting_QueryCountPerTime")} must at the range [{PAGESIZE_MIN}-{PAGESIZE_MAX}].");
            }

            if (_configModel.ClawlerConfigData.QueryInterval < 3000)
            {
                throw new ClawlerConfigException($"{ResourceHelper.FindKey("Setting_CrawlerSetting_QueryInterval")} must at the bigger than 3000 ms.");
            }

        }
        #endregion
    }
}
