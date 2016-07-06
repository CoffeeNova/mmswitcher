using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace mmswitcherAPI.Messangers
{
    internal partial class WebHookManager
    {
        private event EventHandler _gotFocus;
        private event EventHandler _lostFocus;

        public event EventHandler GotFocus
        {
            add
            {
                if (Process != null)
                    EnsureSubscribedToGotFocusEvent(Browser, Process);
                _gotFocus += value;
            }
            remove
            {
                _gotFocus -= value;
                TryUnsubscribeFromGotFocusEvent();
            }
        }

        public event EventHandler LostFocus
        {
            add
            {
                if (Process != null)
                    EnsureSubscribedToLostFocusEvent(Browser, Process);
                _lostFocus += value;
            }
            remove
            {
                _lostFocus -= value;
                TryUnsubscribeFromLostFocusEvent();
            }
        }
    }
}
