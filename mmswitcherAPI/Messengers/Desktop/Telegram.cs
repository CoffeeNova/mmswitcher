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

        protected override MemoryVariableData GetMessagesCounterData()
        {
            var data = new MemoryVariableData();
            data.Address = (IntPtr)Constants.TELEGRAM_NEWMESSAGESCOUNT_MEMORY_BASE_ADDRESS;
            data.Size = Constants.TELEGRAM_NEWMESSAGESCOUNT_MEMORY_SIZE;
            data.Offset = 0;
            data.Divider = 256;
            return data;

            //var data = new VariableData();
            //var mainModuleAddress = base._process.MainModule.BaseAddress;

            //var pointer = new IntPtr(Constants.TELEGRAM_NEWMESSAGESCOUNT_MEMORY_BASE_POINTER);
            //var handle = WinApi.OpenProcess(ProcessSecurityAndAccessRights.PROCESS_VM_READ, false, base._process.Id);

            //IntPtr bytesRead;
            //var buffer = new byte[4];
            //WinApi.ReadProcessMemory(handle, pointer, buffer, buffer.Length, out bytesRead);
            //WinApi.CloseHandle(handle);
            //data.Address = (IntPtr)BitConverter.ToInt32(buffer, 0);
            //data.Size = Constants.TELEGRAM_NEWMESSAGESCOUNT_MEMORY_SIZE;
            //data.Offset = Constants.TELEGRAM_NEWMESSAGESCOUNT_MEMORY_OFFSET;
            //return data;
        }

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



        // public static Telegram _instance;
        //private static object _locker = new object();
        private string _trayButtonName = Constants.TELEGRAM_TRAY_BUTTON_NAME;
        private bool _disposed = false;
    }
}
