using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Automation;
using System.Threading;

namespace mmswitcherAPI.Messengers.Desktop
{
    public sealed class Telegram : DesktopMessenger
    {
        public Telegram(Process process)
            : base(process)
        {
            if (process == null)
                throw new ArgumentException();
        }

        //protected override void OnMessageTraced(object sender, IntPtr hWnd, ShellEvents shell) { }

        protected override string TrayButtonName
        {
            get { return _trayButtonName; }
        }

        protected override IntPtr GetMainWindowHandle(Process process)
        {
            return process.MainWindowHandle;
        }

        protected override bool RestoreFromTray()
        {
            var trayButton = TrayButton();

            if ((bool)trayButton.GetCurrentPropertyValue(AutomationElement.IsInvokePatternAvailableProperty))
            {
                var pattern = (InvokePattern)trayButton.GetCurrentPattern(InvokePattern.Pattern);
                pattern.Invoke();
            }
            else
                Tools.SimulateClickUIAutomation(trayButton, UserPromotedNotificationArea, (IntPtr)UserPromotedNotificationArea.Current.NativeWindowHandle, false);
            Thread.Sleep(100);
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                //_locker = null;
                //_instance = null;
            }
            _disposed = true;
            base.Dispose(disposing);
        }

        protected override MessagesVariableLocation MessagesData
        {
            get { return _messagesData; }
        }

        // public static Telegram _instance;
        //private static object _locker = new object();
        private string _trayButtonName = Constants.TELEGRAM_TRAY_BUTTON_NAME;
        private bool _disposed = false;

        private MessagesVariableLocation _messagesData = new MessagesVariableLocation()
        {
            MaxPointerLevel = Constants.MEMORY_BASE_POINTERS_LEVEL,
            Offsets = Constants.TELEGRAM_NEWMESSAGESCOUNT_MEMORY_OFFSETS,
            Size = Constants.TELEGRAM_NEWMESSAGESCOUNT_MEMORY_SIZE
        };
    }
}
