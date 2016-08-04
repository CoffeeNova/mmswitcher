﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Forms;

namespace mmswitcherAPI.winmsg
{
    /// <summary>
    /// Предоставляет интерфейс для мониторинга сообщений Windows
    /// </summary>
    public abstract class MsgMonitor
    {
        private object _control;
        private readonly int _msgNotify;
        private bool _disposed = false;
        private HwndSource _hwndWindow;
        private MControl _msgReceiver;
        private bool _isWpfSpecial = false;

        public delegate void MsgEventHandler(object sender, IntPtr hWnd, ShellEvents shell);

        //Происходит при обнаружении нужного сообщения
        public event MsgEventHandler onMessageTraced;

        /// <summary>
        /// Конструктор для общих сообщений.
        /// </summary>
        /// <param name="window">Окно WPF.</param>
        /// <param name="msgNotify">Значение сообщения. См. http://wiki.winehq.org/List_Of_Windows_Messages </param>
        public MsgMonitor(Window window, int msgNotify)
        {
            _isWpfSpecial = true;
            _control = window;
            _msgNotify = msgNotify;
            _hwndWindow = PresentationSource.FromVisual(_control as Window) as HwndSource;
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
            _control = window;
            _msgNotify = WinApi.RegisterWindowMessage("SHELLHOOK");
            WinApi.RegisterShellHookWindow(new WindowInteropHelper(window).Handle);
            _hwndWindow = PresentationSource.FromVisual(_control as Window) as HwndSource;
            _hwndWindow.AddHook(MessageTrace);
        }

        /// <summary>
        /// Конструктор для общих сообщений.
        /// </summary>
        /// <param name="msgNotify">Значение сообщения. См. http://wiki.winehq.org/List_Of_Windows_Messages. </param>
        public MsgMonitor(int msgNotify)
        {
            _msgReceiver = new MControl();
            _control = _msgReceiver;
            _msgNotify = msgNotify;
            _msgReceiver.onWndProc += MessageTrace;
        }

        /// <summary>
        /// Конструктор для shell сообщений.
        /// </summary>
        public MsgMonitor()
        {
            _msgReceiver = new MControl();
            _control = _msgReceiver;
            _msgNotify = WinApi.RegisterWindowMessage("SHELLHOOK");
            WinApi.RegisterShellHookWindow(_msgReceiver.Handle);
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
                        handler(_control, lParam, (ShellEvents)wParam.ToInt32());
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
                    _control = null;
                    if (_isWpfSpecial)
                        _hwndWindow.RemoveHook(new HwndSourceHook(MessageTrace));
                    _hwndWindow.Dispose();
                    _msgReceiver.Dispose();
                    onMessageTraced = null;
                }

                try
                {
                    if (!_isWpfSpecial)
                        WinApi.DeregisterShellHookWindow(_msgReceiver.Handle);
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

    internal class MControl : Control, IDisposable
    {
        public delegate IntPtr WndProcDelegate(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled);
        public event WndProcDelegate onWndProc;

        protected override void WndProc(ref Message m)
        {
            var handler = onWndProc;
            bool handled = false;
            if (handler != null)
                handler(m.HWnd, m.Msg, m.WParam, m.LParam, ref handled);
            base.WndProc(ref m);
        }
        protected override void Dispose(bool disposing)
        {
            onWndProc = null;
            base.Dispose(disposing);
        }
    }
}


