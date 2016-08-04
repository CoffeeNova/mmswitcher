using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Automation;

namespace mmswitcherAPI.Messengers.Desktop
{
    public sealed class Telegram : DesktopMessenger
    {
        private Telegram(Process process)
            : base(process)
        {
        }

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

        //last tested telegram version was v0.10 03.08.16
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

        //private AutomationElement
        //for telegram focus reciever element is edit contol (chat edit control)
        protected override AutomationElement FocusReciever(IntPtr hWnd)
        {
            return GetChatEditControlManually(hWnd);
        }

        protected override bool IncomeMessagesDetect(AutomationElement element)
        {
            return false;
        }

        protected override string TrayButtonName
        {
            get { return _trayButtonName; }
        }

        public static Telegram _instance;

        private static object _locker = new object();
        private string _trayButtonName = "Telegram Desktop";
    }
}
