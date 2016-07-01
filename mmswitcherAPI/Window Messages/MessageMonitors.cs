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
    public class WindowMessagesMonitor : MsgMonitor
    {
        //конструктор для wpf приложения
        public WindowMessagesMonitor(Window window)
            : base(window) { }
        //общий конструктор
        public WindowMessagesMonitor()
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

    public class SetTextMessageMonitor : MsgMonitor
    {
        //конструктор для wpf приложения
        public SetTextMessageMonitor(Window window)
            : base(window, (int)WindowMessage.WM_SETTEXT) { }

        //общий конструктор
        public SetTextMessageMonitor()
            : base((int)WindowMessage.WM_SETTEXT) { }

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
}

