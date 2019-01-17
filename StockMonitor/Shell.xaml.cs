using GalaSoft.MvvmLight.Messaging;
using Structures.Messengers.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace StockMonitor
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class Shell : Window
    {

        /// <summary>   
        /// 获取鼠标的坐标   
        /// </summary>   
        /// <param name="lpPoint">传址参数，坐标point类型</param>   
        /// <returns>获取成功返回真</returns>   
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetCursorPos(out POINT pt);

        /// <summary>
        /// 鼠标坐标结构体
        /// </summary>
        public struct POINT
        {
            public int X;
            public int Y;
            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        private POINT _POINT;
        private Rect _beforDragRect;
        private Rect _oldPositionRect;
        public Shell()
        {
            InitializeComponent();
            Loaded += Shell_Loaded;
            Subscribe();
        }

        #region private methods
        /// <summary>
        /// 订阅
        /// </summary>
        private void Subscribe()
        {
            Messenger.Default.Register<string>(this, WindowOptions.Min, OnMin);
            Messenger.Default.Register<string>(this, WindowOptions.Close, OnClose);
            Messenger.Default.Register<string>(this, WindowOptions.Normal, OnNormal);
            Messenger.Default.Register<string>(this, WindowOptions.Max, OnMax);
        }


        private void Shell_Loaded(object sender, RoutedEventArgs e)
        {
            _oldPositionRect = new Rect(Left, Top, Width, Height);
        }
        /// <summary>
        /// 还原
        /// </summary>
        /// <param name="obj"></param>
        private void OnNormal(string obj)
        {
            if (_oldPositionRect == default(Rect)) return;

            Top = _oldPositionRect.Top;
            Left = _oldPositionRect.Left;
            Width = _oldPositionRect.Width;
            Height = _oldPositionRect.Height;

            _oldPositionRect = default(Rect);
        }

        /// <summary>
        /// 最大化
        /// </summary>
        /// <param name="obj"></param>
        private void OnMax(string obj)
        {
            _oldPositionRect = new Rect(Left, Top, Width, Height);
            if (Top == 0)
            {
                _oldPositionRect = _beforDragRect;
            }
            Top = 0;
            Left = 0;
            Width = SystemParameters.WorkArea.Width;
            Height = SystemParameters.WorkArea.Height;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="obj"></param>
        private void OnClose(string obj)
        {
            this.Close();
        }

        /// <summary>
        /// 最小化
        /// </summary>
        /// <param name="obj"></param>
        private void OnMin(string obj)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// 校验鼠标是否移至屏幕顶端
        /// </summary>
        /// <returns></returns>
        private bool IsMouseTop()
        {
            POINT p = new POINT();
            GetCursorPos(out p);
            if (p.Y == 0)
                return true;
            return false;
        }

        /// <summary>
        /// 鼠标左键按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //双击最大化
            if (e.ClickCount == 2)
            {
                GetCursorPos(out _POINT);
                e.Handled = true;
                maxRestoreBtn.IsChecked = !maxRestoreBtn.IsChecked;
                return;
            }

            //拖拽
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                POINT beforDragPoint = new POINT();
                GetCursorPos(out beforDragPoint);
                _beforDragRect = new Rect(Left, Top, Width, Height);

                DragMove();

                POINT p = new POINT();
                GetCursorPos(out p);
                if (p.X == _POINT.X && p.Y == _POINT.Y)
                    return;

                //如果拖到置顶则最大化窗体
                if (IsMouseTop())
                {
                    //如果已在最顶部拖拽,则不处理
                    if (maxRestoreBtn.IsChecked == true)
                    {
                        Top = 0;
                        Left = 0;
                        return;
                    }
                    maxRestoreBtn.IsChecked = true;
                }
                else
                {
                    if (_oldPositionRect != default(Rect))
                    {
                        POINT afterDragPoint = new POINT();
                        GetCursorPos(out afterDragPoint);
                        
                        //拖拽超过1个单位则视为拖拽
                        if (Math.Abs(afterDragPoint.X - beforDragPoint.X) <= 1 && Math.Abs(afterDragPoint.Y - beforDragPoint.Y) <= 1)
                        {
                            return;
                        }

                        var top = afterDragPoint.Y - beforDragPoint.Y;
                        var left = afterDragPoint.X - (beforDragPoint.X * _oldPositionRect.Width / SystemParameters.WorkArea.Width);
                        _oldPositionRect = new Rect(left, top, _oldPositionRect.Width, _oldPositionRect.Height);
                        maxRestoreBtn.IsChecked = false;
                    }
                }
            }
        }
        #endregion
        
    }
}