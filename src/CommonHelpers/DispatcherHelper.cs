using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CommonHelpers
{
    public static class DispatcherHelper
    {
        private static Dispatcher _dispatcher = null;
        public static void InitilizeDispatcher()
        {
            if (_dispatcher != null && _dispatcher.Thread.IsAlive)
                return;
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public static void Run(Action action)
        {
            CheckDispatcherInitilized();
            _dispatcher.Invoke(action);
        }

        public static void RunAsync(Action action)
        {
            if (action == null)
                return;
            CheckDispatcherInitilized();
            if (_dispatcher.CheckAccess())
                action();
            else
                _dispatcher.BeginInvoke(action);
        }

        private static void CheckDispatcherInitilized()
        {
            if (_dispatcher == null)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("The DispatcherHelper is not initialized.");
                stringBuilder.AppendLine("Call DispatcherHelper.Initialize() in the static App constructor.");
                throw new InvalidOperationException(stringBuilder.ToString());
            }
        }
    }
}
