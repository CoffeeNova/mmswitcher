using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Automation;
using mmswitcherAPI.Messengers.Web.Browsers;
using System.Collections.ObjectModel;
using mmswitcherAPI.Extensions;

namespace mmswitcherAPI.Messengers.Web
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
        }

        protected override int GetMessagesCount(AutomationElement tab)
        {
            return tab.Current.Name.ParseNumber();
        }

        private bool _disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
            }
            _disposed = true;
            base.Dispose(disposing);
        }

    }
}
