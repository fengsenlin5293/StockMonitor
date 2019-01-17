using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structures
{
    public static class LocalDataPath
    {
        static LocalDataPath()
        {
            LocalStockBasicDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Datas", "StockBasicData.json");
            LocalStockBasicDataSyncTime = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Datas", "LocalStockBasicDataSyncTime.json");
            TopicStockLinkDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Datas", "TopicStockLinkData.json");
            TopicStockLinkDataSyncTime = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Datas", "TopicStockLinkDataSyncTime.json");
        }

        public readonly static string LocalStockBasicDataPath;
        public readonly static string LocalStockBasicDataSyncTime;
        public readonly static string TopicStockLinkDataPath;
        public readonly static string TopicStockLinkDataSyncTime;

    }
}
