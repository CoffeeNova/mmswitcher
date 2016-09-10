using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mmswitcherAPI.AltTabSimulator
{
    internal partial class AltTabHookManager : IDisposable
    {
        public AltTabHookManager()
        {
            WindowsMessagesTrapper.Start();
        }
        private event EventHandler s_ForegroundChanged;
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler ForegroundChanged
        {
            add
            {
                TrySubscribeToForegroundChangedEvent();
                s_ForegroundChanged += value;
            }
            remove
            {
                s_ForegroundChanged -= value;
                TryUnsubscribeFromForegroundChangedEvent();
            }
        }

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                try { TryUnsubscribeFromForegroundChangedEvent(); }
                catch (InvalidOperationException) { }
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AltTabHookManager()
        {
            s_ForegroundChanged = null;
            Dispose(false);
        }
    }
}
