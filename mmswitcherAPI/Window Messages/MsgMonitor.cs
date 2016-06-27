using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Forms;

namespace mmswitcherAPI.winmsg
{
    /// <summary>
    /// Предоставляет интерфейс для мониторинга сообщений Windows
    /// </summary>
    public abstract class MsgMonitor
    {
        private object _window;
        private readonly int _msgNotify;
        private bool _disposed = false;
        private HwndSource _hwndWindow;
        private MControl _msgReceiver;
        private bool _isWpfSpecial = false;

        public delegate void EventHandler(object sender, IntPtr hWnd, Interop.ShellEvents shell);

        //Происходит при обнаружении нужного сообщения
        public event EventHandler onMessageTraced;

        /// <summary>
        /// Конструктор для общих сообщений.
        /// </summary>
        /// <param name="window">Окно WPF.</param>
        /// <param name="msgNotify">Значение сообщения. См. http://wiki.winehq.org/List_Of_Windows_Messages </param>
        public MsgMonitor(Window window, int msgNotify)
        {
            _isWpfSpecial = true;
            _window = window;
            _msgNotify = msgNotify;
            _hwndWindow = PresentationSource.FromVisual(_window as Window) as HwndSource;
            _hwndWindow.AddHook(MessageTrace);
        }

        /// <summary>
        /// Конструктор для shell сообщений. 
        /// </summary>
        /// <param name="window">Окно WPF.</param>
        /// <remarks>Рекомендуется использовать этот конструктор при разработке WPF приложения.</remarks>
        public MsgMonitor(Window window)
        {
            _isWpfSpecial = true;
            _window = window;
            _msgNotify = Interop.RegisterWindowMessage("SHELLHOOK");
            Interop.RegisterShellHookWindow(new WindowInteropHelper(window).Handle);
            _hwndWindow = PresentationSource.FromVisual(_window as Window) as HwndSource;
            _hwndWindow.AddHook(MessageTrace);
        }

        /// <summary>
        /// Конструктор для общих сообщений.
        /// </summary>
        /// <param name="msgNotify">Значение сообщения. См. http://wiki.winehq.org/List_Of_Windows_Messages. </param>
        /// <remarks>Рекомендуется использовать этот конструктор при разработке WPF приложения.</remarks>
        public MsgMonitor(int msgNotify)
        {
            _msgReceiver = new MControl();
            _msgNotify = msgNotify;
            _msgReceiver.onWndProc += MessageTrace;
        }

        /// <summary>
        /// Конструктор для shell сообщений.
        /// </summary>
        public MsgMonitor()
        {
            _msgReceiver = new MControl();
            _msgNotify = Interop.RegisterWindowMessage("SHELLHOOK");
            Interop.RegisterShellHookWindow(_msgReceiver.Handle);
            _msgReceiver.onWndProc += MessageTrace;
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
                    if (_isWpfSpecial)
                        _hwndWindow.RemoveHook(new HwndSourceHook(MessageTrace));
                    else
                        Interop.DeregisterShellHookWindow(_msgReceiver.Handle);
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

    internal class MControl : Control 
    {
        private delegate IntPtr WndProcDelegate(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled);
        public event WndProcDelegate onWndProc;

        protected override void WndProc(ref Message m)
        {
            var handler = onWndProc;
            bool handled = false;
            if (handler != null)
                handler(m.HWnd, m.Msg, m.WParam, m.LParam, ref handled);
            base.WndProc(ref m);
        }
    }
}


