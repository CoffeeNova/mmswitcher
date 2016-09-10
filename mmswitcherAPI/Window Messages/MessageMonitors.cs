using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mmswitcherAPI;
using System.Windows;
using System.Windows.Forms;

namespace mmswitcherAPI.winmsg
{
    /// <summary>
    /// Обнаруживает Windows сообщения создания/уничтожения и изменении активации окон
    /// </summary>
    public class WindowLifeCycle : MsgMonitor
    {
        /// <summary>
        /// Конструктор для WPF приложения
        /// </summary>
        /// <param name="window"></param>
        /// <remarks>Должен выполняться в основном потоке графического интерфейса.</remarks>
        public WindowLifeCycle(Window window)
            : base(window, "SHELLHOOK") { }
        /// <summary>
        /// Общий конструктор.
        /// </summary>
        /// <remarks>Предполагает использование класса <see cref="WindowsMessagesTrapper"/></remarks>
        public WindowLifeCycle()
            : base("SHELLHOOK") { }

        protected override bool MessageRecognize(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Receive shell messages
            switch ((ShellEvents)wParam.ToInt32())
            {
                case ShellEvents.HSHELL_WINDOWCREATED:
                case ShellEvents.HSHELL_WINDOWDESTROYED:
                case ShellEvents.HSHELL_WINDOWACTIVATED:
                    return true;
            }
            return false;
        }

    }

    public class WM_PAINT_Monitor1 : GlobalHookTrapper
    {

        public WM_PAINT_Monitor1(GlobalHookTypes Type, IntPtr hMod, uint dThreadId) : base(Type, hMod, dThreadId) { }

        protected override bool Handle(IntPtr wparam, IntPtr lparam)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("wparam: {0} , lparam: {1}", wparam, lparam));
            return true;
        }

    }
    /// <summary>
    /// Класс перехвата сообщения Windows WM_PAINT
    /// </summary>
    public class WM_PAINT_Monitor : MsgMonitor
    {
        /// <summary>
        /// Конструктор для приложения WPF
        /// </summary>
        /// <param name="window"></param>
        /// <param name="hWnd">Дескриптор элемента, которому предназначено сообщение.</param>
        public WM_PAINT_Monitor(Window window, IntPtr hWnd)
            : base(window, WindowMessage.WM_PAINT)
        {
            WindowHandleControl(hWnd);
            _windowHandle = hWnd;
        }

        /// <summary>
        /// Общий конструктор.
        /// </summary>
        /// <param name="hWnd">Дескриптор элемента, которому предназначено сообщение.</param>
        public WM_PAINT_Monitor(IntPtr hWnd)
            : base(WindowMessage.WM_PAINT)
        {
            WindowHandleControl(hWnd);
            _windowHandle = hWnd;
            try
            {
                var disp = WindowsMessagesTrapper.Dispatcher;
                var mtHandle = (IntPtr)disp.Invoke(new Func<IntPtr>(() => { return (MessagesTrapper as WindowsMessagesTrapper).Handle; }));

                var t =WinApi.PostMessage(_windowHandle, base.MsgNotify[0], IntPtr.Zero, mtHandle);

                var r = t;
            }
            catch { }

        }

        protected override bool MessageRecognize(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            return hwnd.Equals(_windowHandle);
        }

        private void WindowHandleControl(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                throw new ArgumentException("hWnd should not be IntPtr.Zero");
        }
        private IntPtr _windowHandle = IntPtr.Zero;
    }

}

