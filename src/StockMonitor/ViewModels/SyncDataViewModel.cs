using CommonHelpers;
using CommonHelpers.Config;
using DotNet.Log4Net;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using HttpHelpers;
using JsonHelpers;
using Newtonsoft.Json.Linq;
using StockHelpers;
using Structures;
using Structures.Dto;
using Structures.JsonModels;
using Structures.Messengers.Args;
using Structures.Messengers.Tokens;
using Structures.Stocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StockMonitor.ViewModels
{
    public class SyncDataViewModel : ViewModelBase
    {
        #region Field

        private ConfigJsonModel _configModel;

        #endregion
        public SyncDataViewModel()
        {
            StartSyncDataCommand = new RelayCommand(OnStartSyncData);
            _configModel = ConfigJsonHelper.GetConfigModel();

            InitData();
        }

        #region properties

        private bool _canStartSyncData = true;
        /// <summary>
        /// 是否可以同步
        /// </summary>
        public bool CanStartSyncData
        {
            get { return _canStartSyncData; }
            set { Set(() => CanStartSyncData, ref _canStartSyncData, value); }
        }


        private bool _isSyncTopicStockLinkData = true;
        /// <summary>
        /// 是否勾选了同步题材与个股关系数据
        /// </summary>
        public bool IsSyncTopicStockLinkData
        {
            get { return _isSyncTopicStockLinkData; }
            set { Set(() => IsSyncTopicStockLinkData, ref _isSyncTopicStockLinkData, value); }
        }

        private bool _isSyncStockBasicData = true;
        /// <summary>
        /// 是否勾选了同步个股基本数据
        /// </summary>
        public bool IsSyncStockBasicData
        {
            get { return _isSyncStockBasicData; }
            set { Set(() => IsSyncStockBasicData, ref _isSyncStockBasicData, value); }
        }

        private bool? _isSyncTopicStockLinkDataSuccess;
        /// <summary>
        /// 是否同步题材个股关联数据成功
        /// </summary>
        public bool? IsSyncTopicStockLinkDataSuccess
        {
            get { return _isSyncTopicStockLinkDataSuccess; }
            set { Set(() => IsSyncTopicStockLinkDataSuccess, ref _isSyncTopicStockLinkDataSuccess, value); }
        }

        private string _topicStockLinkDataSyncTime;
        /// <summary>
        /// 题材个股关联数据同步时间
        /// </summary>
        public string TopicStockLinkDataSyncTime
        {
            get { return _topicStockLinkDataSyncTime; }
            set { Set(() => TopicStockLinkDataSyncTime, ref _topicStockLinkDataSyncTime, value); }
        }


        private bool? _isSyncStockBasicDataSuccess;
        /// <summary>
        /// 是否同步基本数据成功
        /// </summary>
        public bool? IsSyncStockBasicDataSuccess
        {
            get { return _isSyncStockBasicDataSuccess; }
            set { Set(() => IsSyncStockBasicDataSuccess, ref _isSyncStockBasicDataSuccess, value); }
        }


        private string _stockBasicDataSyncTime;
        /// <summary>
        /// 个股基本数据同步时间
        /// </summary>
        public string StockBasicDataSyncTime
        {
            get { return _stockBasicDataSyncTime; }
            set { Set(() => StockBasicDataSyncTime, ref _stockBasicDataSyncTime, value); }
        }


        #endregion

        #region Commands

        public ICommand StartSyncDataCommand { get; set; }

        #endregion

        #region Command Execute

        private async void OnStartSyncData()
        {
            CanStartSyncData = false;

            if (IsSyncStockBasicData)
            {
                //同步的时间和数据分开保存的原因在于:初始化的时候,只想获取同步时间,不需要数据,避免内存浪费(字符串无法释放)
                Messenger.Default.Send(new StatusBarArgs(true, ResourceHelper.FindKey("StatusBar_Message_SyncingStockBasicData")), StatusBarToken.UpdateStatus);
                //同步个股基本数据
                try
                {
                    var stockDetails = await Task.Run(() => CommonStockDataManager.Instance.GetStockDetailModel(true));
                    var basicDataJsonModel = new LocalStockBasicDataModel() { SyncTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                    var jsonData = JsonHelper.SerializeObject(basicDataJsonModel);
                    if (!string.IsNullOrEmpty(jsonData))
                    {
                        //保存同步时间json
                        if (File.Exists(LocalDataPath.LocalStockBasicDataSyncTime))
                            File.Delete(LocalDataPath.LocalStockBasicDataSyncTime);
                        File.AppendAllText(LocalDataPath.LocalStockBasicDataSyncTime, jsonData);

                        //保存数据json
                        basicDataJsonModel.StockDetailModels = stockDetails;
                        jsonData = JsonHelper.SerializeObject(basicDataJsonModel);
                        if (File.Exists(LocalDataPath.LocalStockBasicDataPath))
                            File.Delete(LocalDataPath.LocalStockBasicDataPath);
                        File.AppendAllText(LocalDataPath.LocalStockBasicDataPath, jsonData);

                        this.StockBasicDataSyncTime = basicDataJsonModel.SyncTime;
                        this.IsSyncStockBasicDataSuccess = true;
                    }
                }
                catch (Exception ex)
                {
                    this.IsSyncStockBasicDataSuccess = false;
                    Messenger.Default.Send(new StatusBarArgs(true, string.Format(ResourceHelper.FindKey("StatusBar_Message_SyncStockBasicDataFailed"), ex.Message)), StatusBarToken.UpdateStatus);
                    LogBuilder.Logger.Error("同步个股基本数据 - 失败", ex);
                }
            }


            if (IsSyncTopicStockLinkData)
            {

                Messenger.Default.Send(new StatusBarArgs(true, ResourceHelper.FindKey("StatusBar_Message_SyncingStockTopicLinkData")), StatusBarToken.UpdateStatus);
                //同步题材与个股关系数据
                try
                {
                    WebProxy proxy = null;
                    if (_configModel.IsUseProxy)
                    {
                        proxy = new WebProxy(_configModel?.ProxyData?.IP, _configModel.ProxyData.Port);
                    }

                    var sectionModels = await Task.Run(() => GetSectionModels(proxy));

                    var stockDetails = await Task.Run(() => CommonStockDataManager.Instance.GetStockDetailModel(false));
                    //所有个股代码
                    var stocks = stockDetails.Select(x => x.Code).ToList();

                    var models = await Task.Run(() => GetStorageStockSectionModels(proxy, stocks, sectionModels));

                    var topicDataJsonModel = new TopicStockLinkDataModel() { SyncTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };

                    var jsonTopicStockData = JsonHelper.SerializeObject(topicDataJsonModel);
                    if (!string.IsNullOrEmpty(jsonTopicStockData))
                    {
                        if (File.Exists(LocalDataPath.TopicStockLinkDataSyncTime))
                            File.Delete(LocalDataPath.TopicStockLinkDataSyncTime);
                        File.AppendAllText(LocalDataPath.TopicStockLinkDataSyncTime, jsonTopicStockData);

                        topicDataJsonModel.StorageStockSectionModels = models;
                        jsonTopicStockData = JsonHelper.SerializeObject(topicDataJsonModel);
                        if (File.Exists(LocalDataPath.TopicStockLinkDataPath))
                            File.Delete(LocalDataPath.TopicStockLinkDataPath);
                        File.AppendAllText(LocalDataPath.TopicStockLinkDataPath, jsonTopicStockData);

                        this.TopicStockLinkDataSyncTime = topicDataJsonModel.SyncTime;
                        this.IsSyncTopicStockLinkDataSuccess = true;
                    }
                }
                catch (Exception ex)
                {
                    this.IsSyncTopicStockLinkDataSuccess = false;
                    Messenger.Default.Send(new StatusBarArgs(true, string.Format(ResourceHelper.FindKey("StatusBar_Message_SyncStockTopicLinkDataFailed"), ex.Message)), StatusBarToken.UpdateStatus);
                    LogBuilder.Logger.Error("同步题材与个股关系数据 - 失败", ex);
                }
            }

            if (IsSyncStockBasicData || IsSyncTopicStockLinkData)
            {
                //如果更新了数据文件,则停止正在进行的监控
                Messenger.Default.Send(string.Empty, MonitorOptions.StopMonitor);
            }

            Messenger.Default.Send(new StatusBarArgs(true, ResourceHelper.FindKey("StatusBar_Message_Completed")), StatusBarToken.UpdateStatus);
            Messenger.Default.Send(new StatusBarArgs(false, ""), StatusBarToken.UpdateStatus);

            CanStartSyncData = true;
        }

        /// <summary>
        /// 获取所有板块集合
        /// </summary>
        /// <param name="proxy"></param>
        /// <returns></returns>
        private List<SectionModel> GetSectionModels(WebProxy proxy)
        {
            string url = "http://quote.eastmoney.com/center/sidemenu.json";
            var headers = new WebHeaderCollection();
            headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9");

            var response = HttpHelper.CreateGetHttpResponse(url, 6000, new CustomerHeader(), headers, proxy);
            var responseString = HttpHelper.GetResponseString(response);
            var datas = JsonHelper.DeserializeJsonToObject<JObject[]>(responseString);
            //hsbroad
            var hsbroad = datas.FirstOrDefault(data => data.First.ToString().Contains("hsbroad"));
            var section = hsbroad.SelectToken("next").SelectMany(x => x.SelectToken("next"));
            var sectionModels = section.Select(x => JsonHelper.DeserializeJsonToObject<SectionModel>(x.ToString())).Where(x => x.Show);
            string preUrl = "http://quote.eastmoney.com";
            foreach (var item in sectionModels)
            {
                var title = item.Title;
                var newurl = preUrl + item.Href;
            }
            return sectionModels.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="proxy"></param>
        /// <param name="stocks"></param>
        /// <param name="sectionModels"></param>
        /// <returns></returns>
        private List<StorageStockSectionModel> GetStorageStockSectionModels(WebProxy proxy, List<string> stocks, List<SectionModel> sectionModels)
        {
            var headers = new WebHeaderCollection();
            headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9");

            var pr = "http://nufm.dfcfw.com/EM_Finance2014NumericApplication/JS.aspx?";
            var urlParam = "cb=jQuery112406821645128125025_1537783921470&type=CT&token=4f1862fc3b5e77c150a2b985b12db0fd&sty=FCOIATC&js=(%7Bdata%3A%5B(x)%5D%2CrecordsFiltered%3A(tot)%7D)&cmd=C.{0}&st=(ChangePercent)&sr=-1&p=1&ps=2000&_=1537783921481";

            var regex = new Regex("[{](.*?)[}]{1,}");
            List<Tuple<string, string, StockBase>> dic = new List<Tuple<string, string, StockBase>>();

            int i = 0;
            string preUrl = "http://quote.eastmoney.com";
            foreach (var item in sectionModels)
            {
                string newUrl = pr + string.Format(urlParam, item.Key.Replace("boards-", "").Trim());
                var response = HttpHelper.CreateGetHttpResponse(newUrl, 60000, new CustomerHeader(), headers, proxy);
                var responseString = HttpHelper.GetResponseString(response);

                var match = regex.Match(responseString);
                var queryStockDetailResponseModel = JsonHelper.DeserializeJsonToObject<QueryStockDetailResponseModel>(match.Value);
                var stockDetailModels = DtoHelper.GetStockDetailModel(queryStockDetailResponseModel.Datas);
                stockDetailModels.ForEach(x =>
                {
                    dic.Add(new Tuple<string, string, StockBase>(item.Title, preUrl + item.Href, x));
                });
                Messenger.Default.Send(new StatusBarArgs(true, string.Format(ResourceHelper.FindKey("StatusBar_Message_SyncingStockTopicLinkDataProgress"), i, sectionModels.Count)), StatusBarToken.UpdateStatus);
                Console.WriteLine(i);
                i++;
                Thread.Sleep(200);
            }

            List<StorageStockSectionModel> models = new List<StorageStockSectionModel>();
            stocks.ToList().ForEach(x =>
            {
                StorageStockSectionModel temp = GetStorageStockSectionModel(dic, x);
                temp.StockCode = x;
                temp.StockName = CommonStockDataManager.Instance.GetStockDetailModel(x).Name;
                models.Add(temp);
            });
            return models;
        }

        private StorageStockSectionModel GetStorageStockSectionModel(List<Tuple<string, string, StockBase>> dic, string code)
        {
            StorageStockSectionModel model = new StorageStockSectionModel();
            var finds = dic.Where(x => x.Item3.Code.Equals(code));
            foreach (var item in finds)
            {
                StorageSection section = new StorageSection();
                section.Title = item.Item1;
                section.Url = item.Item2;
                model.Sections.Add(section);
            }
            return model;
        }

        #endregion

        #region private methods

        private void InitData()
        {
            if (File.Exists(LocalDataPath.LocalStockBasicDataSyncTime))
            {
                var jsonString = File.ReadAllText(LocalDataPath.LocalStockBasicDataSyncTime);
                var obj = JsonHelper.DeserializeJsonToObject<LocalStockBasicDataModel>(jsonString);
                this.StockBasicDataSyncTime = obj.SyncTime;
            }

            if (File.Exists(LocalDataPath.TopicStockLinkDataSyncTime))
            {
                var jsonString = File.ReadAllText(LocalDataPath.TopicStockLinkDataSyncTime);
                var obj = JsonHelper.DeserializeJsonToObject<TopicStockLinkDataModel>(jsonString);
                this.TopicStockLinkDataSyncTime = obj.SyncTime;
            }
        }

        #endregion
    }
}
