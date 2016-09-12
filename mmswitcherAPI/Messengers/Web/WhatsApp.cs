using System;
using System.Diagnostics;
using System.Windows.Automation;
using mmswitcherAPI.Extensions;

namespace mmswitcherAPI.Messengers.Web
{
    public sealed class WebWhatsApp : WebMessenger, IDisposable
    {
        public override Messenger Messenger
        {
            get { return Messenger.WebWhatsApp; }
        }

        public WebWhatsApp(Process browserProcess)
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
