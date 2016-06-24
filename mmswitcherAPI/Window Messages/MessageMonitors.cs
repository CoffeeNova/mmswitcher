using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mmswitcherAPI;
using System.Windows;

namespace mmswitcherAPI.winmsg
{
    /// <summary>
    /// Обнаруживает Windows сообщения создания/уничтожения и изменении активации окон
    /// </summary>
    public class WindowMessagesMonitor : MsgMonitor
    {
        public WindowMessagesMonitor(Window window)
            : base(window) { }

        protected override bool MessageRecognize(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Receive shell messages
            switch ((Interop.ShellEvents)wParam.ToInt32())
            {
                case Interop.ShellEvents.HSHELL_WINDOWCREATED:
                case Interop.ShellEvents.HSHELL_WINDOWDESTROYED:
                case Interop.ShellEvents.HSHELL_WINDOWACTIVATED:
                    return true;
            }
            return false;
        }
    }

    public class SetTextMessageMonitor : MsgMonitor
    {
        public SetTextMessageMonitor(Window window)
            : base(window, (int)WindowMessage.WM_SETTEXT) { }

        protected override bool MessageRecognize(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Receive shell messages
            switch ((Interop.ShellEvents)wParam.ToInt32())
            {
                case Interop.ShellEvents.HSHELL_WINDOWCREATED:
                case Interop.ShellEvents.HSHELL_WINDOWDESTROYED:
                case Interop.ShellEvents.HSHELL_WINDOWACTIVATED:
                    return true;
            }
            return false;
        }
    }
}

