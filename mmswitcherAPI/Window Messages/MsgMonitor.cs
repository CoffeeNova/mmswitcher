using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Threading;

namespace mmswitcherAPI.winmsg
{
    /// <summary>
    /// Предоставляет интерфейс для мониторинга сообщений Windows
    /// </summary>
    public abstract class MsgMonitor : IDisposable
    {
        /// <summary>
        /// Конструктор для общих сообщений.
        /// </summary>
        /// <param name="window">Окно WPF.</param>
        /// <param name="msgNotify">Значения сообщений. См. http://wiki.winehq.org/List_Of_Windows_Messages </param>
        /// <exception cref="InvalidOperationException">Вызывающий поток не является потоком графического интерфейса окна <paramref name="window"/>.</example>
        public MsgMonitor(Window window, params WindowMessage[] msgNotify)
        {
            UIThreadException(window);
            _isWpfSpecial = true;
            _messagesTrapper = window;
            MsgNotify = msgNotify.Cast<uint>().ToArray(); ;
            _hwndWindow = PresentationSource.FromVisual(MessagesTrapper as Window) as HwndSource;
            _hwndWindow.AddHook(MessageTrace);
        }

        ///// <summary>
        ///// Конструктор для shell сообщений. 
        ///// </summary>
        ///// <param name="window">Окно WPF.</param>
        ///// <remarks>Рекомендуется использовать этот конструктор при разработке WPF приложения.</remarks>
        //public MsgMonitor(Window window)
        //{
        //    _isWpfSpecial = true;
        //    _control = window;
        //    _msgNotify = new int[1] { WinApi.`("SHELLHOOK") };
        //    WinApi.RegisterShellHookWindow(new WindowInteropHelper(window).Handle);
        //    _hwndWindow = PresentationSource.FromVisual(_control as Window) as HwndSource;
        //    _hwndWindow.AddHook(MessageTrace);
        //}

        /// <summary>
        /// Конструктор для общих сообщений.
        /// </summary>
        /// <param name="msgNotify">Значения сообщений. См. http://wiki.winehq.org/List_Of_Windows_Messages. </param>
        public MsgMonitor(params WindowMessage[] msgNotify)
            : this()
        {
            _messagesTrapper = _msgReceiver;
            MsgNotify = msgNotify.Cast<uint>().ToArray();
            WindowsMessagesTrapper.onWndProc += MessageTrace;
        }

        /// <summary>
        /// Конструктор для shell сообщений.
        /// </summary>
        //public MsgMonitor()
        //{
        //    WindowsMessagesHandler.Start();
        //    _msgReceiver = WindowsMessagesHandler.Instance;
        //    var disp = WindowsMessagesHandler.Dispatcher;
        //    _control = _msgReceiver;
        //    _msgNotify = new int[1] { WinApi.RegisterWindowMessage("SHELLHOOK") };
        //    var handle = (IntPtr)disp.Invoke(new Func<IntPtr>(() => { return _msgReceiver.Handle; }));
        //    WinApi.RegisterShellHookWindow(handle);
        //    WindowsMessagesHandler.onWndProc += MessageTrace;
        //}

        /// <summary>
        /// Конструктор для пользовательских сообщений.
        /// </summary>
        /// <param name="CustomMessage"></param>
        public MsgMonitor(params string[] CustomMessage)
            : this()
        {
            var registredHandles = CustomMessage.Select<string, uint>((s, i) => { return WinApi.RegisterWindowMessage(s); });
            MsgNotify = registredHandles.ToArray();
            if (CustomMessage.Any((s) => s.Equals("SHELLHOOK")))
                RegisterShellHookWindow();
            WindowsMessagesTrapper.onWndProc += MessageTrace;
        }

        /// <summary>
        /// Конструктор для пользовательских сообщений.
        /// </summary>
        /// <param name="CustomMessage"></param>
        /// <param name="window">Окно Wpf.</param>
        /// <exception cref="InvalidOperationException">Вызывающий поток не является потоком графического интерфейса окна <paramref name="window"/>.</example>
        public MsgMonitor(Window window, params string[] CustomMessage)
        {
            UIThreadException(window);
            _isWpfSpecial = true;
            _messagesTrapper = window;
            var registredHandles = CustomMessage.Select<string, uint>((s, i) => { return WinApi.RegisterWindowMessage(s); });
            MsgNotify = registredHandles.ToArray();
            if (CustomMessage.Any((s) => s.Equals("SHELLHOOK")))
                WinApi.RegisterShellHookWindow(new WindowInteropHelper(window).Handle);
            _hwndWindow = PresentationSource.FromVisual(MessagesTrapper as Window) as HwndSource;
            _hwndWindow.AddHook(MessageTrace);
        }

        private MsgMonitor()
        {
            _msgReceiver = WindowsMessagesTrapper.Instance;
            _messagesTrapper = _msgReceiver;
        }

        private void UIThreadException(Window window)
        {
            try { var handle = window.WindowState; }
            catch (InvalidOperationException ex)
            {
                if (string.Equals(ex.Message, "The calling thread cannot access this object because a different thread owns it."))
                    throw new InvalidOperationException("Should use this constructor only in UI thread.", ex);
            }
        }

        private void RegisterShellHookWindow()
        {
            if (shellHookWindowRegistered)
                return;
            var disp = WindowsMessagesTrapper.Dispatcher;
            disp.Invoke(new Action(() => { WinApi.RegisterShellHookWindow(_msgReceiver.Handle);}));
            shellHookWindowRegistered = true;
        }

        private void UnregisterShellHookWindow()
        {
            if (!shellHookWindowRegistered)
                return;
            var disp = WindowsMessagesTrapper.Dispatcher;
            disp.Invoke(new Action(() => { WinApi.DeregisterShellHookWindow(_msgReceiver.Handle); }));
            shellHookWindowRegistered = false;
        }

        //Callback функция хука
        private IntPtr MessageTrace(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (MsgNotify.Any((x) => x == msg))
                if (MessageRecognize(hwnd, msg, wParam, lParam, ref handled))
                {
                    var handler = onMessageTraced;
                    if (handler != null)
                        handler(MessagesTrapper, lParam, (ShellEvents)wParam.ToInt32());
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
                    try
                    {
                        if (!_isWpfSpecial)
                            UnregisterShellHookWindow();
                    }
                    catch { }
                    _messagesTrapper = null;
                    if (_isWpfSpecial)
                        _hwndWindow.RemoveHook(new HwndSourceHook(MessageTrace));
                    WindowsMessagesTrapper.onWndProc -= MessageTrace;

                    if (_hwndWindow != null)
                        _hwndWindow.Dispose();
                    _msgReceiver = null;
                    onMessageTraced = null;
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MsgMonitor()
        {
            Dispose(false);
        }

        /// <summary>
        /// Объект формы, которая перехватывает сообщения Windows.
        /// <remarks>Может быть представлена классом <see cref="mmswitcherAPI.winmsg.WindowsMessagesTrapper"/>, либо <see cref="System.Windows.Window"/>, в зависимости от конструктора.</remarks>
        /// </summary>
        public object MessagesTrapper { get { return _messagesTrapper; } }

        public readonly uint[] MsgNotify;
        private bool _disposed = false;
        private HwndSource _hwndWindow;
        private WindowsMessagesTrapper _msgReceiver;
        private bool _isWpfSpecial = false;
        private bool shellHookWindowRegistered = false;
        private object _messagesTrapper;
        public delegate void MsgEventHandler(object sender, IntPtr hWnd, ShellEvents shell);

        //Происходит при обнаружении нужного сообщения
        public event MsgEventHandler onMessageTraced;
    }

}


