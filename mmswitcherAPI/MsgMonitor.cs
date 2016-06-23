using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Forms;
using System.Collections.Generic;

namespace mmswitcherAPI
{
    public abstract class MsgMonitor
    {
        private static object _window;
        private readonly int _msgNotify;

        public delegate void EventHandler(object sender, IntPtr hWnd, Interop.ShellEvents shell);
        public event EventHandler onMessageTrace;

        public MsgMonitor(Window window, int msgNotify)
        {
            _window = window;
            _msgNotify = msgNotify;
            HwndSource source = PresentationSource.FromVisual(_window as Window) as HwndSource;
            source.AddHook(MessageTrace);
        }
        public MsgMonitor(Form window, int msgNotify)
        {
            _window = window;
            _msgNotify = msgNotify;
            Interop.RegisterShellHookWindow((_window as Form).Handle);

        }

        private IntPtr MessageTrace(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == _msgNotify)
                if (MessageRecognize(hwnd, msg, wParam, lParam, ref handled))
                {
                    var handler = onMessageTrace;
                    if (handler != null)
                        handler(_window, lParam, (Interop.ShellEvents)wParam.ToInt32());
                }
            return IntPtr.Zero;
        }

        protected abstract bool MessageRecognize(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled);

    }
}


//if (msg == _msgNotify)
//                {
//                    // Receive shell messages
//                    switch ((Interop.ShellEvents)wParam.ToInt32())
//                    {
//                        case Interop.ShellEvents.HSHELL_WINDOWCREATED:
//                        case Interop.ShellEvents.HSHELL_WINDOWDESTROYED:
//                        case Interop.ShellEvents.HSHELL_WINDOWACTIVATED:
//                            var handler = onMessageTrace;
//                            if (handler != null)
//                                handler(_window, lParam, (Interop.ShellEvents)wParam.ToInt32());
//                            break;
//                    }
//                }