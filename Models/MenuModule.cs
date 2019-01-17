using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class MenuModule : ViewModelBase
    {
        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set { Set(() => IsSelected, ref _isSelected, value); }
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set { Set(() => Name, ref _name, value); }
        }

        private string _iconUri;

        public string IconUri
        {
            get { return _iconUri; }
            set { Set(() => IconUri, ref _iconUri, value); }
        }

        private string _navigationInfo;

        public string NavigationInfo
        {
            get { return _navigationInfo; }
            set { Set(() => NavigationInfo, ref _navigationInfo, value); }
        }

    }
}
