using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelpers.Config
{
    public static class ConfigAppSettingHelper
    {
        public const string Language = "Language";

        public static string GetAppConfig(string strKey)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(strKey))
            {
                return ConfigurationManager.AppSettings[strKey];
            }
            return null;
        }


        public static void UpdateAppConfig(string newKey, string newValue)
        {
            bool isModified = false;
            if (ConfigurationManager.AppSettings.AllKeys.Contains(newKey))
            {
                isModified = true;
            }

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (isModified)
            {
                config.AppSettings.Settings.Remove(newKey);
            }
            config.AppSettings.Settings.Add(newKey, newValue);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
