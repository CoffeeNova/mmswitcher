using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Automation;
using mmswitcherAPI.Messangers.Web.Browsers;

namespace mmswitcherAPI.Messangers.Web
{
    public sealed class WebSkype : WebMessenger, IDisposable
    {
        public override Messenger Messenger
        {
            get { return Messenger.Skype; }
        }

        WebMessengerHookManager _hManager;
        
        public WebSkype(Process browserProcess)
            : base(browserProcess)
        {
            if (browserProcess == null)
                throw new ArgumentException();
            
            if (_hManager == null)
                InitHookManager(browserProcess);
            _hManager = new WebMessengerHookManager(base.WindowHandle, _browserSet);
        }

        private void InitHookManager(Process browserProcess)
        {
            if (_hManager != null) return;
            _hManager = new WebMessengerHookManager(base._windowHandle, _browserSet);
        }

        protected override bool IncomeMessagesDetect(AutomationElement tab)
        {
            return tab.Current.Name.StartsWith("(") ? true : false;
        }

        protected override AutomationElement GetIncomeMessageAutomationElement(IntPtr hWnd)
        {
            return _browserSet.MessengerIncomeMessageAutomationElement(hWnd);
        }

        private bool disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                _hManager.Dispose();
            }
            disposed = true;
            base.Dispose(disposing);
        }

    }
}
