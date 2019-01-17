using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StockMonitor.Controls
{
    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
    /// 元素中: 
    ///
    ///     xmlns:MyNamespace="clr-namespace:StockMonitor.Controls"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
    /// 元素中: 
    ///
    ///     xmlns:MyNamespace="clr-namespace:StockMonitor.Controls;assembly=StockMonitor.Controls"
    ///
    /// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
    /// 并重新生成以避免编译错误: 
    ///
    ///     在解决方案资源管理器中右击目标项目，然后依次单击
    ///     “添加引用”->“项目”->[选择此项目]
    ///
    ///
    /// 步骤 2)
    /// 继续操作并在 XAML 文件中使用控件。
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    public class SyncStateControl : Control
    {
        static SyncStateControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SyncStateControl), new FrameworkPropertyMetadata(typeof(SyncStateControl)));
        }

        #region 依赖属性


        /// <summary>
        /// 同步状态
        /// </summary>
        public SyncState SyncState
        {
            get { return (SyncState)GetValue(SyncStateProperty); }
            set { SetValue(SyncStateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SyncState.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SyncStateProperty =
            DependencyProperty.Register("SyncState", typeof(SyncState), typeof(SyncStateControl), new PropertyMetadata(SyncState.NotSync));

        /// <summary>
        /// 状态消息
        /// </summary>
        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Message.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(SyncStateControl), new PropertyMetadata(string.Empty));

        /// <summary>
        /// 同步图标uri
        /// </summary>
        public Uri StateIconSource
        {
            get { return (Uri)GetValue(StateIconSourceProperty); }
            set { SetValue(StateIconSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StateIconSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StateIconSourceProperty =
            DependencyProperty.Register("StateIconSource", typeof(Uri), typeof(SyncStateControl), new PropertyMetadata(default(Uri)));

        /// <summary>
        /// 是否使用动画
        /// </summary>
        public bool IsStartAnimation
        {
            get { return (bool)GetValue(IsStartAnimationProperty); }
            set { SetValue(IsStartAnimationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsStartAnimation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsStartAnimationProperty =
            DependencyProperty.Register("IsStartAnimation", typeof(bool), typeof(SyncStateControl), new PropertyMetadata(false));


        #endregion
    }
}
