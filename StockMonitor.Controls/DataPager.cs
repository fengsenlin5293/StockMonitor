using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StockMonitor.Controls
{
    [TemplatePart(Name = "PART_HomePage", Type = typeof(Button))]
    [TemplatePart(Name = "PART_PrePageButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_CenterPageItemsControl", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "PART_NextPageButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_LastPage", Type = typeof(Button))]
    [TemplatePart(Name = "PART_GoTextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_GoButton", Type = typeof(Button))]
    public class DataPager : Control
    {
        static DataPager()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DataPager), new FrameworkPropertyMetadata(typeof(DataPager)));
        }
        #region field

        private Button _HomePageBtn;
        private Button _PrePageButton;
        private ItemsControl _PageCenterItemsControl;
        private Button _NextPageButton;
        private Button _LastPageBtn;
        private TextBox _GoTextBox;
        private Button _GoButtonBtn;
        #endregion
        private string _guid = System.Guid.NewGuid().ToString();
        public string Guid
        {
            get { return _guid; }
            private set { _guid = value; }
        }


        #region  control DependencyProperties

        /// <summary>
        /// current page index
        /// </summary>
        public int CurrentPageIndex
        {
            get { return (int)GetValue(CurrentPageIndexProperty); }
            set { SetValue(CurrentPageIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentPageIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentPageIndexProperty =
            DependencyProperty.Register("CurrentPageIndex", typeof(int), typeof(DataPager), new PropertyMetadata(1, DataPagerPropertyChanged));

        /// <summary>
        /// max count show on page
        /// </summary>
        public int PageSize
        {
            get { return (int)GetValue(PageSizeProperty); }
            set { SetValue(PageSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PageSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PageSizeProperty =
            DependencyProperty.Register("PageSize", typeof(int), typeof(DataPager), new PropertyMetadata(20, DataPagerPropertyChanged));

        /// <summary>
        /// count of datas
        /// </summary>
        public int TotalCount
        {
            get { return (int)GetValue(TotalCountProperty); }
            set { SetValue(TotalCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TotalCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TotalCountProperty =
            DependencyProperty.Register("TotalCount", typeof(int), typeof(DataPager), new PropertyMetadata(0, DataPagerPropertyChanged));

        /// <summary>
        /// count of pages
        /// </summary>
        public int PageCount
        {
            get { return (int)GetValue(PageCountProperty); }
            set { SetValue(PageCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PageCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PageCountProperty =
            DependencyProperty.Register("PageCount", typeof(int), typeof(DataPager), new PropertyMetadata(0));

        /// <summary>
        /// max page numbers 
        /// </summary>
        public int MaxCenterPageCount
        {
            get { return (int)GetValue(MaxCenterPageCountProperty); }
            set { SetValue(MaxCenterPageCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxCenterPageCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxCenterPageCountProperty =
            DependencyProperty.Register("MaxCenterPageCount", typeof(int), typeof(DataPager), new PropertyMetadata(0));

        #endregion


        #region override

        protected void Init()
        {
            if (_PageCenterItemsControl != null)
            {
                PageCount = TotalCount % PageSize == 0 ? TotalCount / PageSize : TotalCount / PageSize + 1;

                var source = this.GetShowPageModels(PageCount, CurrentPageIndex, MaxCenterPageCount);
                _PageCenterItemsControl.ItemsSource = source;
                this.PageIndexChanged();
            }
        }

        private void PageIndexChanged()
        {
            if (_PageCenterItemsControl == null)
                return;
            var pageModels = _PageCenterItemsControl.ItemsSource as List<PageModel>;
            foreach (var pageModel in pageModels)
            {
                pageModel.IsSelected = pageModel.Index == CurrentPageIndex;
            }
            if (this._HomePageBtn != null)
            {
                this._HomePageBtn.IsEnabled = IsPreAndHomePageBtnEnable(CurrentPageIndex);
            }
            if (this._PrePageButton != null)
            {
                this._PrePageButton.IsEnabled = IsPreAndHomePageBtnEnable(CurrentPageIndex);
            }

            if (this._NextPageButton != null)
            {
                this._NextPageButton.IsEnabled = IsNextAndLastPageBtnEnable(CurrentPageIndex);
            }
            if (this._LastPageBtn != null)
            {
                this._LastPageBtn.IsEnabled = IsNextAndLastPageBtnEnable(CurrentPageIndex);
            }
        }

        public override void OnApplyTemplate()
        {
            if (this._HomePageBtn != null)
            {
                _HomePageBtn.Click -= _HomePageBtn_Click;
            }

            if (this._PrePageButton != null)
            {
                _PrePageButton.Click -= _PrePageButton_Click;
            }

            if (_PageCenterItemsControl != null)
            {
                _PageCenterItemsControl.RemoveHandler(RadioButton.CheckedEvent, new RoutedEventHandler(RadioButtonChecked));
                _PageCenterItemsControl.Items.Clear();
            }

            if (this._NextPageButton != null)
            {
                this._NextPageButton.Click -= _NextPageButton_Click;
            }

            if (this._LastPageBtn != null)
            {
                this._LastPageBtn.Click -= _LastPageBtn_Click;
            }

            if (this._GoButtonBtn != null)
            {
                this._GoButtonBtn.Click -= _GoButtonBtn_Click;
            }

            if (this._GoTextBox != null)
            {
                this._GoTextBox.Text = string.Empty;
                this._GoTextBox.PreviewKeyDown -= _GoTextBox_PreviewKeyDown;
            }

            base.OnApplyTemplate();
            this._HomePageBtn = (base.GetTemplateChild("PART_HomePage")) as Button;
            if (_HomePageBtn != null)
            {
                _HomePageBtn.Click += _HomePageBtn_Click;
            }

            this._PrePageButton = (base.GetTemplateChild("PART_PrePageButton")) as Button;
            if (this._PrePageButton != null)
            {
                _PrePageButton.Click += _PrePageButton_Click;
            }

            this._PageCenterItemsControl = (base.GetTemplateChild("PART_CenterPageItemsControl")) as ItemsControl;
            if (_PageCenterItemsControl != null)
            {
                _PageCenterItemsControl.AddHandler(RadioButton.CheckedEvent, new RoutedEventHandler(RadioButtonChecked));
            }

            this._NextPageButton = (base.GetTemplateChild("PART_NextPageButton")) as Button;
            if (this._NextPageButton != null)
            {
                this._NextPageButton.Click += _NextPageButton_Click;
            }

            this._LastPageBtn = (base.GetTemplateChild("PART_LastPage")) as Button;
            if (this._LastPageBtn != null)
            {
                this._LastPageBtn.Click += _LastPageBtn_Click;
            }

            this._GoButtonBtn = (base.GetTemplateChild("PART_GoButton")) as Button;
            if (this._GoButtonBtn != null)
            {
                this._GoButtonBtn.Click += _GoButtonBtn_Click;
            }

            this._GoTextBox = (base.GetTemplateChild("PART_GoTextBox")) as TextBox;
            if (_GoTextBox != null)
            {
                this._GoTextBox.PreviewKeyDown += _GoTextBox_PreviewKeyDown;
            }

            Init();
        }

        #endregion

        #region private methods

        private bool IsPreAndHomePageBtnEnable(int currentIndex)
        {
            return currentIndex != 1;
        }

        private bool IsNextAndLastPageBtnEnable(int currentIndex)
        {
            return currentIndex != PageCount;
        }

        private void RadioButtonChecked(object sender, RoutedEventArgs e)
        {
            var radioButton = e.OriginalSource as RadioButton;
            if (radioButton == null)
                return;

            int pageIndex = 1;
            bool isParseSuccess = int.TryParse(radioButton.Content.ToString(), out pageIndex);
            if (!isParseSuccess)
                return;

            if (CurrentPageIndex != pageIndex)
                CurrentPageIndex = pageIndex;
        }

        private static void DataPagerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataPager = d as DataPager;
            dataPager.Init();
        }

        private List<PageModel> GetShowPageModels(int pageCount, int currentPageIndex, int maxCenterPageCount)
        {
            List<PageModel> pageModels = new List<PageModel>();
            if (pageCount <= maxCenterPageCount)
            {
                for (int i = 1; i <= pageCount; i++)
                {
                    pageModels.Add(new PageModel() { Index = i });
                }
                return pageModels;
            }

            int mid = maxCenterPageCount / 2;
            int startIndex = 1;

            if (currentPageIndex <= mid + 1)
            {
                startIndex = 1;
            }
            else if (currentPageIndex >= pageCount - mid + 1)
            {
                startIndex = pageCount - maxCenterPageCount + 1;
            }
            else
            {
                startIndex = currentPageIndex - mid;
            }

            for (int i = 0; i < maxCenterPageCount; i++)
            {
                pageModels.Add(new PageModel() { Index = startIndex + i });
            }
            return pageModels;
        }

        private void _GoButtonBtn_Click(object sender, RoutedEventArgs e)
        {
            GoToPage();
        }


        private void _GoTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GoToPage();
            }
        }

        private void GoToPage()
        {
            if (_GoTextBox == null)
                return;
            int goIndex = 1;
            bool isParseSuccess = int.TryParse(_GoTextBox.Text, out goIndex);
            if (!isParseSuccess)
                return;
            if (CurrentPageIndex != goIndex && goIndex <= PageCount)
                CurrentPageIndex = goIndex;
        }

        private void _LastPageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPageIndex != PageCount)
                CurrentPageIndex = PageCount;
        }

        private void _NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPageIndex < PageCount)
                CurrentPageIndex++;
        }

        private void _PrePageButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPageIndex > 1)
                CurrentPageIndex--;
        }

        private void _HomePageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPageIndex != 1)
                CurrentPageIndex = 1;
        }


        #endregion
    }

    public class PageModel : INotifyPropertyChanged
    {
        #region properties
        private int _index;

        public int Index
        {
            get { return _index; }
            set
            {
                _index = value;
                RaisePropertyChanged("Index");
            }
        }

        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string parameter)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(parameter));
        }

    }
}
