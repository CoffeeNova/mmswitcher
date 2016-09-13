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
    public class Skype : DesktopMessenger
    {
        public Skype(Process process)
            : base(process)
        {
            if (process == null)
                throw new ArgumentException();
        }
        //public static Skype Instance(Process process)
        //{
        //    if (_instance == null)
        //    {
        //        lock (_locker)
        //        {
        //            if (_instance == null)
        //                _instance = new Skype(process);
        //        }
        //    }
        //    return _instance;
        //}

        private AutomationElement GetChatEditControlManually(IntPtr hWnd)
        {
            try
            {
                var mainWindowAe = AutomationElement.FromHandle(hWnd);
                var firstCustomChild = mainWindowAe.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Custom))[1];
                var secondCustomChild = firstCustomChild.FindFirst(TreeScope.Children, Condition.TrueCondition);
                var editControl = secondCustomChild.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));
                return editControl;
            }
            catch { throw new Exception("Cannot find chat edit control."); }
        }

        protected override MemoryVariableData GetMessagesCounterData()
        {
            var data = new MemoryVariableData();
            var mainModuleAddress = base._process.MainModule.BaseAddress;

            var pointer = new IntPtr(Constants.SKYPE_NEWMESSAGESCOUNT_MEMORY_BASE_POINTER);
            var handle = WinApi.OpenProcess(ProcessSecurityAndAccessRights.PROCESS_VM_READ, false, base._process.Id);

            IntPtr bytesRead;
            var buffer = new byte[4];
            WinApi.ReadProcessMemory(handle, pointer, buffer, buffer.Length, out bytesRead);
            WinApi.CloseHandle(handle);
            data.Address = (IntPtr)BitConverter.ToInt32(buffer, 0);
            data.Size = Constants.SKYPE_NEWMESSAGESCOUNT_MEMORY_SIZE;
            data.Offset = Constants.SKYPE_NEWMESSAGESCOUNT_MEMORY_OFFSET;
            data.Divider = 0;
            return data;
        }

        protected override string TrayButtonName
        {
            get { return _trayButtonName; }
        }

        protected override List<AutomationElement> GetFocusRecieverAutomationElement(IntPtr hWnd)
        {
            var allSkypeElements = base.MessengerAE.FindAll(TreeScope.Subtree, Condition.TrueCondition);
            var elementsAsList = new AutomationElement[allSkypeElements.Count];
            allSkypeElements.CopyTo(elementsAsList, 0);
            return elementsAsList.ToList();
        }

        protected override IntPtr GetMainWindowHandle(Process process)
        {
            var handle = process.MainWindowHandle;
            var windowContent = Tools.GetClassName(handle);
            if (windowContent == Constants.SKYPE_MAINWINDOW_CLASSNAME)
                return handle;
            else
                return IntPtr.Zero;
        }

        protected override bool RestoreFromTray()
        {
            var trayButton = TrayButton();
            Tools.SimulateClickUIAutomation(trayButton, UserPromotedNotificationArea, (IntPtr)UserPromotedNotificationArea.Current.NativeWindowHandle, true);
            Thread.Sleep(100);
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            lock (_locker)
            {
                if (_disposed)
                    return;
                if (disposing)
                {
                    // _locker = null;
                    // _instance = null;
                }
                _disposed = true;
            }

            base.Dispose(disposing);
        }

        //public static Skype _instance;
        private static object _locker = new object();
        private string _trayButtonName = Constants.SKYPE_TRAY_BUTTON_NAME;
        private bool _disposed = false;
    }
}
