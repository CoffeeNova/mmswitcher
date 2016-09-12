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

        //public static Telegram Instance(Process process)
        //{
        //    if (_instance == null)
        //    {
        //        lock (_locker)
        //        {
        //            if (_instance == null)
        //                _instance = new Telegram(process);
        //        }
        //    }
        //    return _instance;
        //}

        protected override void OnMessageTraced(object sender, IntPtr hWnd, ShellEvents shell) { }

        protected override VariableData GetMessagesCounterData()
        {
            var data = new VariableData();
            data.Address = (IntPtr)Constants.TELEGRAM_NEWMESSAGESCOUNT_MEMORY_BASE_ADDRESS;
            data.Size = Constants.TELEGRAM_NEWMESSAGESCOUNT_MEMORY_SIZE;
            data.Offset = 0;
            return data;
        }

        protected override string TrayButtonName
        {
            get { return _trayButtonName; }
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



        // public static Telegram _instance;
        //private static object _locker = new object();
        private string _trayButtonName = Constants.TELEGRAM_TRAY_BUTTON_NAME;
        private bool _disposed = false;
    }
}
