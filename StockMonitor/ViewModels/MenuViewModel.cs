using CommonHelpers;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;
using Models;
using Structures;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StockMonitor.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        public MenuViewModel()
        {
            MenuModules = new ObservableCollection<MenuModule>();

            ModuleSwitchCommand = new RelayCommand(OnModuleSwitch);

            InitData();
        }



        private void InitData()
        {
            MenuModules.Add(new MenuModule()
            {
                Name = ResourceHelper.FindKey("Menu_HomePage"),
                IsSelected = true,
                IconUri = "pack://application:,,,/Resource;component/Images/Common/Menu_HomePage.png",
                NavigationInfo = NavigationPages.MonitorPage
            });

            MenuModules.Add(new MenuModule()
            {
                Name = ResourceHelper.FindKey("Menu_Setting"),
                IsSelected = false,
                IconUri = "pack://application:,,,/Resource;component/Images/Common/Menu_setting.png",
                NavigationInfo = NavigationPages.SettingPage
            });

            MenuModules.Add(new MenuModule()
            {
                Name = ResourceHelper.FindKey("Menu_SyncData"),
                IsSelected = false,
                IconUri = "pack://application:,,,/Resource;component/Images/Common/Menu_SyncData.png",
                NavigationInfo = NavigationPages.SyncDataPage
            });

            MenuModules.Add(new MenuModule()
            {
                Name = ResourceHelper.FindKey("Menu_About"),
                IsSelected = false,
                IconUri = "pack://application:,,,/Resource;component/Images/Common/Menu_About.png",
                NavigationInfo = NavigationPages.AboutPage
            });
        }


        #region 属性

        public ObservableCollection<MenuModule> MenuModules { get; private set; }

        #endregion

        #region ICommands

        public ICommand ModuleSwitchCommand { get; private set; }

        #endregion

        #region Command Execute

        private void OnModuleSwitch()
        {
            var navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
            var selected = MenuModules.FirstOrDefault(m => m.IsSelected);
            if (selected != null)
            {
                DispatcherHelper.RunAsync(() => navigationService.NavigateTo(selected.NavigationInfo));
            }
        }

        #endregion



    }
}
