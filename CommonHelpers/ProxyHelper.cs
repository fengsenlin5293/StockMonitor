using HttpHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommonHelpers
{
    public static class ProxyHelper
    {
        /// <summary>
        /// 东方财富获取10只个股的基本数据数据url
        /// </summary>
        private static string _url = "http://nufm.dfcfw.com/EM_Finance2014NumericApplication/JS.aspx?cb=jQuery11240010094446092091314_1536245516320&type=CT&token=4f1862fc3b5e77c150a2b985b12db0fd&sty=FCOIATC&js=(%7Bdata%3A%5B(x)%5D%2CrecordsFiltered%3A(tot)%7D)&cmd=C._A&st=(ChangePercent)&sr=-1&p=1&ps=10&_=1536245516321";
        static ProxyHelper()
        {

        }
        /// <summary>
        /// 检验代理的是否可用
        /// </summary>
        /// <param name="ip">ip</param>
        /// <param name="port">端口</param>
        /// <param name="timeout">超时(ms)</param>
        /// <returns></returns>
        public static bool CheckProxyUsefull(string ip, int port, int timeout)
        {
            try
            {
                WebProxy proxy = new WebProxy(ip, port);
                var headers = new WebHeaderCollection();
                headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9");
                var response = HttpHelper.CreateGetHttpResponse(_url, timeout, new CustomerHeader(), headers, proxy);
                var responseString = HttpHelper.GetResponseString(response);

                var regex = new Regex("[{](.*?)[}]{1,}"); //get json string
                var match = regex.Match(responseString);

                if (!match.Value.Contains("recordsFiltered"))
                {
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
