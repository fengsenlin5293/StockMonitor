using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;
using Structures;
using Structures.Messengers.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StockMonitor.ViewModels
{
    public class ShellViewModel : ViewModelBase
    {
        public ShellViewModel()
        {
            ShellLoadCommand = new RelayCommand(OnShellLoad);
            MinCommand = new RelayCommand(OnMin);
            CloseCommand = new RelayCommand(OnClose);
            MaxCommand = new RelayCommand(OnMax);
            NormalCommand = new RelayCommand(OnNormal);

        }     

        #region Commands

        public ICommand ShellLoadCommand { get; set; }
        public ICommand MinCommand { get; set; }
        public ICommand CloseCommand { get; set; }
        public ICommand MaxCommand { get; set; }
        public ICommand NormalCommand { get; set; }

        #endregion

        #region Command Execute

        private void OnMin()
        {
            Messenger.Default.Send(string.Empty, WindowOptions.Min);
        }

        private void OnClose()
        {
            Messenger.Default.Send(string.Empty, WindowOptions.Close);
        }

        private void OnMax()
        {
            Messenger.Default.Send(string.Empty, WindowOptions.Max);
        }

        private void OnNormal()
        {
            Messenger.Default.Send(string.Empty, WindowOptions.Normal);
        }

        private void OnShellLoad()
        {            
            var menuVM = ServiceLocator.Current.GetInstance<MenuViewModel>();
            var selectedMenu = menuVM?.MenuModules.FirstOrDefault(m => m.IsSelected);

            var navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
            navigationService?.NavigateTo(selectedMenu?.NavigationInfo);
        }
        #endregion


        #region private methods

      

        #endregion
    }
}
