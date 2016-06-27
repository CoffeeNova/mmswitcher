using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mmswitcherAPI.AltTabSimulator
{
    public static partial class HookManager
    {
        private static event EventHandler s_ForegroundChanged;
        /// <summary>
        /// 
        /// </summary>
        public static event EventHandler ForegroundChanged
        {
            add
            {
                EnsureSubscribedToForegroundChangedEvent();
                s_ForegroundChanged += value;
            }
            remove
            {
                s_ForegroundChanged -= value;
                TryUnsubscribeFromForegroundChangedEvent();
            }
        }
    }
}
