using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Service
{
    public class NavigationService : ViewModelBase, INavigationService
    {
        private readonly Dictionary<string, Uri> _pagesByKey;
        private readonly Dictionary<string, UIElement> _cacheViews = new Dictionary<string, UIElement>();
        private readonly List<string> _historic;

        private string _currentPageKey;

        #region properties

        public string CurrentPageKey
        {
            get { return _currentPageKey; }
            private set
            {
                Set(() => CurrentPageKey, ref _currentPageKey, value);
            }
        }
        public object Parameter { get; private set; }
        #endregion

        #region public methods
        public NavigationService()
        {
            _pagesByKey = new Dictionary<string, Uri>();
            _historic = new List<string>();
        }

        public void GoBack()
        {
            if (_historic.Count > 1)
            {
                _historic.RemoveAt(_historic.Count - 1);
            }
            NavigateTo(_historic.Last(), "Back");
        }

        public void NavigateTo(string pageKey)
        {
            NavigateTo(pageKey, "Next");
        }

        public void NavigateTo(string pageKey, object parameter)
        {
            lock (_pagesByKey)
            {
                if (!_pagesByKey.ContainsKey(pageKey))
                {
                    throw new ArgumentException(string.Format("No such Ppage: {0} ", pageKey), "pageKey");
                }
                CurrentPageKey = pageKey;

                var frame = GetDescendantFromName(Application.Current.MainWindow, "MainFrame") as Frame;

                if (frame != null)
                {
                    if (_cacheViews.ContainsKey(pageKey))
                    {
                        frame.Content = _cacheViews[pageKey];
                    }
                    else
                    {
                        frame.Source = _pagesByKey[pageKey];
                        frame.LoadCompleted += Frame_LoadCompleted;
                    }
                }


                Parameter = parameter;
                if (parameter.ToString().Equals("Next"))
                {
                    _historic.Add(pageKey);
                }
            }
        }

        public void Configure(string key, Uri pageType)
        {
            lock (_pagesByKey)
            {
                if (_pagesByKey.ContainsKey(key))
                {
                    _pagesByKey[key] = pageType;
                }
                else
                {
                    _pagesByKey.Add(key, pageType);
                }
            }
        }


        #endregion

        #region private methods


        private void Frame_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            var frame = sender as Frame;
            frame.LoadCompleted -= Frame_LoadCompleted;
            lock (_cacheViews)
            {
                if (_cacheViews.ContainsKey(CurrentPageKey))
                {
                    _cacheViews[CurrentPageKey] = frame.Content as UIElement;
                }
                else
                {
                    _cacheViews.Add(CurrentPageKey, frame.Content as UIElement);
                }
            }
        }

        private FrameworkElement GetDescendantFromName(DependencyObject parent, string name)
        {
            var count = VisualTreeHelper.GetChildrenCount(parent);
            if (count < 1)
            {
                return null;
            }

            for (int i = 0; i < count; i++)
            {
                var frameworkElement = VisualTreeHelper.GetChild(parent, i) as FrameworkElement;
                if (frameworkElement != null)
                {
                    if (frameworkElement.Name == name)
                    {
                        return frameworkElement;
                    }

                    frameworkElement = GetDescendantFromName(frameworkElement, name);
                    if (frameworkElement != null)
                    {
                        return frameworkElement;
                    }
                }
            }
            return null;
        }

        #endregion

    }
}
