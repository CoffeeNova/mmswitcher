using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Automation;
using mmswitcherAPI.Messangers.Web.Browsers;
using System.Collections.ObjectModel;

namespace mmswitcherAPI.Messangers.Web
{
    public sealed class WebSkype : WebMessenger, IDisposable
    {
        public override Messenger Messenger
        {
            get { return Messenger.Skype; }
        }

        public  WebSkype(Process browserProcess)
            : base(browserProcess)
        {
            if (browserProcess == null)
                throw new ArgumentException();

            OnFocusChangedSubscribe();
            OnMessageProcessingSubscribe();
        }

        protected override bool IncomeMessagesDetect(AutomationElement tab)
        {
            return tab.Current.Name.StartsWith("(") ? true : false;
        }

        private bool disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
            }
            disposed = true;
            base.Dispose(disposing);
        }

    }
}
