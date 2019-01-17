using CommonHelpers.Config;
using HttpHelpers;
using JsonHelpers;
using Structures;
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
using System.Threading.Tasks;

namespace StockHelpers
{
    public class CommonStockDataManager
    {
        private bool _isQueryed = false;
        private string _url;
        private bool _isLoadedSection = false;
        private bool _isLoadedLocalBasicStockData = false;
        private string _storageSectionPath;
        private string _stockBasicPath;
        private ConfigJsonModel _configModel;
        /// <summary>
        /// 所有股票的缓存
        /// </summary>
        private ConcurrentDictionary<string, StockDetailModel> _cacheDic = new ConcurrentDictionary<string, StockDetailModel>();

        private CommonStockDataManager()
        {
            _url = "http://nufm.dfcfw.com/EM_Finance2014NumericApplication/JS.aspx?cb=jQuery11240010094446092091314_1536245516320&type=CT&token=4f1862fc3b5e77c150a2b985b12db0fd&sty=FCOIATC&js=(%7Bdata%3A%5B(x)%5D%2CrecordsFiltered%3A(tot)%7D)&cmd=C._A&st=(ChangePercent)&sr=-1&p=1&ps=3700&_=1536245516321";
            _configModel = ConfigJsonHelper.GetConfigModel();
            _storageSectionPath = LocalDataPath.TopicStockLinkDataPath;
            _stockBasicPath = LocalDataPath.LocalStockBasicDataPath;
        }
        private readonly static CommonStockDataManager _instance = new CommonStockDataManager();
        public static CommonStockDataManager Instance
        {
            get
            {
                return _instance;
            }
        }

        /// <summary>
        /// 加载本地个股基本数据
        /// </summary>
        /// <returns></returns>
        public bool LoadStockDetailModel()
        {
            try
            {
                var stockBasicDataString = File.ReadAllText(_stockBasicPath);
                var localStockBasicDataModel = JsonHelper.DeserializeJsonToObject<LocalStockBasicDataModel>(stockBasicDataString);
                List<StockDetailModel> datas = localStockBasicDataModel.StockDetailModels;
                _cacheDic.Clear();
                datas.ForEach(x => _cacheDic.TryAdd(x.Code, x));
                _isLoadedLocalBasicStockData = true;
                return true;
            }
            catch (Exception ex)
            {
                _isLoadedLocalBasicStockData = false;
                return false;
            }

        }

        /// <summary>
        /// 获取所有个股基本数据
        /// </summary>
        /// <returns></returns>
        public List<StockDetailModel> GetStockDetailModel(bool isRealTimeQuery)
        {
            if (isRealTimeQuery)
            {
                _isQueryed = true;
                List<StockDetailModel> datas = GetDetailModelsFromWebQuery();
                _cacheDic.Clear();
                datas.ForEach(x => _cacheDic.TryAdd(x.Code, x));
                return _cacheDic.Values.ToList();
            }

            if (!_isQueryed && !_isLoadedLocalBasicStockData)
            {
                _isQueryed = true;
                List<StockDetailModel> datas = GetDetailModelsFromWebQuery();
                _cacheDic.Clear();
                datas.ForEach(x => _cacheDic.TryAdd(x.Code, x));
            }
            return _cacheDic.Values.ToList();
        }


        /// <summary>
        /// 根据股票代码获取个股基本数据
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public StockDetailModel GetStockDetailModel(string code)
        {
            return _cacheDic[code];
        }


        private ConcurrentQueue<StorageSection> _storageSectionCache = new ConcurrentQueue<StorageSection>();
        private ConcurrentDictionary<string, StorageStockSectionModel> _cacheSectionDic = new ConcurrentDictionary<string, StorageStockSectionModel>();

        /// <summary>
        /// 加载本地题材与个股关系数据
        /// </summary>
        /// <returns></returns>
        public bool LoadStorageSections()
        {
            if (_isLoadedSection)
                return true;
            try
            {
                _isLoadedSection = true;
                LoadLocalStorageSection();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void LoadLocalStorageSection()
        {
            var json = File.ReadAllText(_storageSectionPath);
            var topicStockLinkDataModel = JsonHelper.DeserializeJsonToObject<TopicStockLinkDataModel>(json);
            topicStockLinkDataModel.StorageStockSectionModels.ForEach(x => _cacheSectionDic.TryAdd(x.StockCode, x));
            _storageSectionCache = new ConcurrentQueue<StorageSection>(_cacheSectionDic.Values.SelectMany(x => x.Sections).Distinct(new BKComparer()));
        }

        /// <summary>
        /// 获取个股所属题材的对象集合
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public List<StorageSection> GetStorageSections(string code)
        {
            var reulst = new List<StorageSection>();
            try
            {
                if (!_isLoadedSection)
                {
                    _isLoadedSection = true;
                    LoadLocalStorageSection();
                }
                var model = _cacheSectionDic[code];
                if (model != null)
                    reulst = new List<StorageSection>(model.Sections);
            }
            catch (Exception ex)
            {
                return new List<StorageSection>();
            }

            return reulst;
        }

        /// <summary>
        /// 获取个股所属题材的字符串
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public string GetStorageSectionsStr(string code)
        {
            return string.Join(",", GetStorageSections(code).Select(x => x.Title));
        }

        /// <summary>
        /// 获取板块的Url
        /// </summary>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public string GetStorageSectionUrl(string sectionName)
        {
            return _storageSectionCache.FirstOrDefault(x => x.Title.Equals(sectionName))?.Url;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public List<string> GetSectionCodes(string sectionName)
        {
            List<string> result = new List<string>();
            foreach (var item in _cacheSectionDic.Values)
            {
                if (item.Sections.Any(x => x.Title.Contains(sectionName)))
                    result.Add(item.StockCode);
            }
            return result.Distinct().ToList();
        }

        #region private methods
        /// <summary>
        /// 获取实时个股
        /// </summary>
        /// <returns></returns>
        private List<StockDetailModel> GetDetailModelsFromWebQuery()
        {
            var headers = new WebHeaderCollection();
            headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9");

            WebProxy proxy = null;
            if (_configModel.IsUseProxy)
            {
                proxy = new WebProxy(_configModel?.ProxyData?.IP, _configModel.ProxyData.Port);
            }

            var response = HttpHelper.CreateGetHttpResponse(_url, 10000, new CustomerHeader(), headers, proxy);
            var responseString = HttpHelper.GetResponseString(response);

            var regex = new Regex("[{](.*?)[}]{1,}"); //get json string
            var match = regex.Match(responseString);
            var queryStockDetailResponseModel = JsonHelper.DeserializeJsonToObject<QueryStockDetailResponseModel>(match.Value);
            var datas = DtoHelper.GetStockDetailModel(queryStockDetailResponseModel.Datas);
            return datas;
        }

        #endregion
    }

    class BKComparer : IEqualityComparer<StorageSection>
    {
        public bool Equals(StorageSection x, StorageSection y)
        {
            if (x.Title == y.Title && x.Url == y.Url)
                return true;
            return false;
        }

        public int GetHashCode(StorageSection obj)
        {
            return obj.GetHashCode();
        }
    }
}
