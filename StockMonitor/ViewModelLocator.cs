using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;
using Service;
using StockMonitor.ViewModels;
using Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMonitor
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);


            SimpleIoc.Default.Register<ShellViewModel>();
            SimpleIoc.Default.Register<MenuViewModel>();
            SimpleIoc.Default.Register<SyncDataViewModel>();
            SimpleIoc.Default.Register<SettingViewModel>();
            SimpleIoc.Default.Register<StatusBarVeiwModel>();
            SimpleIoc.Default.Register<MonitorViewModel>();
            SimpleIoc.Default.Register<AboutViewModel>();

            var navigationService = CreateNavigationService();
            SimpleIoc.Default.Register<INavigationService>(() => navigationService);
        }

        private INavigationService CreateNavigationService()
        {
            var navigationService = new NavigationService();

            navigationService.Configure(NavigationPages.SettingPage, new Uri("/StockMonitor;component/Views/SettingPage.xaml", UriKind.Relative));
            navigationService.Configure(NavigationPages.MonitorPage, new Uri("/StockMonitor;component/Views/MonitorPage.xaml", UriKind.Relative));
            navigationService.Configure(NavigationPages.SyncDataPage, new Uri("/StockMonitor;component/Views/SyncDataPage.xaml", UriKind.Relative));
            navigationService.Configure(NavigationPages.AboutPage, new Uri("/StockMonitor;component/Views/AboutPage.xaml", UriKind.Relative));


            return navigationService;
        }

        public bool IsDataSource { get; set; }

        public ShellViewModel ShellViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ShellViewModel>();
            }
        }

        public MenuViewModel MenuViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MenuViewModel>();
            }
        }

        public SyncDataViewModel SyncDataViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SyncDataViewModel>();
            }
        }

        public SettingViewModel SettingViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SettingViewModel>();
            }
        }

        public StatusBarVeiwModel StatusBarVeiwModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<StatusBarVeiwModel>();
            }
        }

        public MonitorViewModel MonitorViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MonitorViewModel>();
            }
        }

        public AboutViewModel AboutViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<AboutViewModel>();
            }
        }


    }
}
