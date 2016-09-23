using System;
using System.Diagnostics;
using System.Windows.Automation;
using mmswitcherAPI.Extensions;

namespace mmswitcherAPI.Messengers.Web
{
    public sealed class WebTelegram : WebMessenger, IDisposable
    {
        public override Messenger Messenger
        {
            get { return Messenger.WebTelegram; }
        }

        public WebTelegram(Process browserProcess)
            : base(browserProcess)
        {
            if (browserProcess == null)
                throw new ArgumentException();
        }

        protected override int? GetMessagesCount(AutomationElement ae)
        {
            string name = ae.Current.Name;
            if (!name.Contains(base._browserSet.MessengerCaption))
                return null;
            return ae.Current.Name.ParseNumber();
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
