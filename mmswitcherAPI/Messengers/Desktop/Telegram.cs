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
        private Telegram(Process process) : base(process) { }

        public static Telegram Instance(Process process)
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    if (_instance == null)
                        _instance = new Telegram(process);
                }
            }
            return _instance;
        }

        ////last tested telegram version was v0.10 03.08.16
        //private AutomationElement GetChatEditControlManually(IntPtr hWnd)
        //{
        //    try
        //    {
        //        var mainWindowAe = AutomationElement.FromHandle(hWnd);
        //        var firstCustomChild = mainWindowAe.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Custom))[1];
        //        var secondCustomChild = firstCustomChild.FindFirst(TreeScope.Children, Condition.TrueCondition);
        //        var editControl = secondCustomChild.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));
        //        return editControl;
        //    }
        //    catch { throw new Exception("Cannot find chat edit control."); }
        //}

        ////private AutomationElement
        ////for telegram focus reciever element is edit contol (chat edit control)
        //protected override AutomationElement FocusReciever(IntPtr hWnd)
        //{
        //    return GetChatEditControlManually(hWnd);
        //}

        //protected override AutomationElement GetFocusRecieverAutomationElement(IntPtr hWnd)
        //{
        //    if (hWnd == null)
        //        throw new ArgumentNullException("hWnd");
        //    if (hWnd == IntPtr.Zero)
        //        throw new ArgumentException("Window handle should not be IntPtr.Zero");

        //    var isVisible = WinApi.ShowWindow(hWnd, ShowWindowEnum.Show);
        //    if (!isVisible)
        //    {
        //        RestoreFromTray();
        //        Thread.Sleep(100);
        //    }

        //    return FocusReciever(hWnd);
        //}

        //protected override AutomationElement GetIncomeMessageAutomationElement(IntPtr hWnd)
        //{
        //    return TrayButton();
        //    //return base.GetIncomeMessageAutomationElement(hWnd);
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
                _locker = null;
                _instance = null;
            }
            _disposed = true;
            base.Dispose(disposing);
        }



        public static Telegram _instance;
        private static object _locker = new object();
        private string _trayButtonName = Constants.TELEGRAM_TRAY_BUTTON_NAME;
        private bool _disposed = false;
    }
}
