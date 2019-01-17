using CommonHelpers;
using CommonHelpers.Config;
using DotNet.Log4Net;
using Interfaces.CommonInterfaces;
using Service;
using Structures;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace StockMonitor
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            LogBuilder.InitLogger("StockMonitorLogger", true);
            DispatcherHelper.InitilizeDispatcher();
            Application.Current.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            base.OnStartup(e);

            var lang = ConfigAppSettingHelper.GetAppConfig(ConfigAppSettingHelper.Language);

            ILanguageService languageService = new LanguageService();

            if (lang.Equals(LanguageResourceKey.English))
            {
                languageService.LoadLanguage(lang);
            }
            else
            {
                languageService.LoadLanguage(LanguageResourceKey.Chinese);
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogBuilder.Logger.Error("Occured a UnhandledException.", e.ExceptionObject as Exception);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogBuilder.Logger.Error("Occured a DispatcherUnhandledException.", e.Exception);
        }


    }
}
