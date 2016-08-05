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
        //конструктор для wpf приложения
        public WindowLifeCycle(Window window)
            : base(window) { }
        //общий конструктор
        public WindowLifeCycle()
            : base() { }

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

    public class MW_PAINT_Monitor : GlobalHookTrapper
    {
        public MW_PAINT_Monitor(GlobalHookTypes Type) : base(Type) { }

        protected override bool Handle(IntPtr wparam, IntPtr lparam)
        {
            throw new NotImplementedException();
        }

    }
    ///// <summary>
    ///// Класс перехвата сообщения Windows WM_PAINT
    ///// </summary>
    //public class WM_PAINT_Monitor : MsgMonitor
    //{
    //    /// <summary>
    //    /// Конструктор для приложения WPF
    //    /// </summary>
    //    /// <param name="window"></param>
    //    /// <param name="hWnd">Дескриптор элемента, которому предназначено сообщение.</param>
    //    public WM_PAINT_Monitor(Window window, IntPtr hWnd)
    //        : base(window, (int)WindowMessage.WM_PAINT) 
    //    {
    //        WindowHandleControl(hWnd);
    //        _windowHandle = hWnd;
    //    }

    //    /// <summary>
    //    /// Общий конструктор.
    //    /// </summary>
    //    /// <param name="hWnd">Дескриптор элемента, которому предназначено сообщение.</param>
    //    public WM_PAINT_Monitor(IntPtr hWnd)
    //        : base((int)WindowMessage.WM_PAINT) 
    //    {
    //        WindowHandleControl(hWnd);
    //        _windowHandle = hWnd;
    //    }

    //    protected override bool MessageRecognize(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    //    {
    //        return hwnd.Equals(_windowHandle);
    //    }

    //    private void WindowHandleControl(IntPtr hWnd)
    //    {
    //        if (hWnd == IntPtr.Zero)
    //            throw new ArgumentException("hWnd should not be IntPtr.Zero");
    //    }
    //    private IntPtr _windowHandle = IntPtr.Zero;
    //}

}

