using JsonHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelpers.Config
{
    public static class ConfigJsonHelper
    {
        private static ConfigJsonModel _configModel;
        private static string _configPath;
        static ConfigJsonHelper()
        {
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "config.json");
        }
        public static bool LoadedAppConfig()
        {
            if (!File.Exists(_configPath))
                return false;

            var jsonData = File.ReadAllText(_configPath);
            _configModel = JsonHelper.DeserializeJsonToObject<ConfigJsonModel>(jsonData);
            return true;
        }

        public static ConfigJsonModel GetConfigModel()
        {
            if (_configModel == null)
                LoadedAppConfig();
            return _configModel;
        }

        public static void SaveConfig()
        {
            if (_configModel == null)
                return;
            var jsonString = JsonHelper.SerializeObject(_configModel);
            if (File.Exists(_configPath))
                File.Delete(_configPath);
            File.AppendAllText(_configPath, jsonString);
        }
    }
}
