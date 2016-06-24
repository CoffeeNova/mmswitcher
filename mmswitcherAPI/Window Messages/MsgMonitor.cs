using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;
using System.Collections.Generic;

namespace mmswitcherAPI.winmsg
{
    /// <summary>
    /// Предоставляет интерфейс для мониторинга сообщений Windows
    /// </summary>
    public abstract class MsgMonitor
    {
        private static Window _window;
        private readonly int _msgNotify;
        private bool _disposed = false;
        private HwndSource _hwndWindow;

        public delegate void EventHandler(object sender, IntPtr hWnd, Interop.ShellEvents shell);

        //Происходит при обнаружении нужного сообщения
        public event EventHandler onMessageTraced;

        /// <summary>
        /// Конструктор для общих сообщений
        /// </summary>
        /// <param name="window">Окно WPF.</param>
        /// <param name="msgNotify">Значение сообщения. См. http://wiki.winehq.org/List_Of_Windows_Messages</param>
        public MsgMonitor(Window window, int msgNotify)
        {
            _window = window;
            _msgNotify = msgNotify;
            _hwndWindow = PresentationSource.FromVisual(_window) as HwndSource;
            _hwndWindow.AddHook(MessageTrace);
        }

        /// <summary>
        /// Конструктор для shell сообщений
        /// </summary>
        /// <param name="window">Окно WPF.</param>
        public MsgMonitor(Window window)
        {
            _window = window;
            _msgNotify = Interop.RegisterWindowMessage("SHELLHOOK");
            Interop.RegisterShellHookWindow(new WindowInteropHelper(window).Handle);
            _hwndWindow = PresentationSource.FromVisual(_window) as HwndSource;
            _hwndWindow.AddHook(MessageTrace);
        }

        //Callback функция хука
        private IntPtr MessageTrace(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == _msgNotify)
                if (MessageRecognize(hwnd, msg, wParam, lParam, ref handled))
                {
                    var handler = onMessageTraced;
                    if (handler != null)
                        handler(_window, lParam, (Interop.ShellEvents)wParam.ToInt32());
                }
            return IntPtr.Zero;
        }

        /// <summary>
        /// Функция отбора необходимых сообщений
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <param name="handled"></param>
        /// <returns></returns>
        protected abstract bool MessageRecognize(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {

                }

                try
                {
                    _hwndWindow.RemoveHook(new HwndSourceHook(MessageTrace));
                }
                catch { }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            //GC.SuppressFinalize(this);
        }

        ~MsgMonitor()
        {
            Dispose(false);
        }
    }
}


