using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mmswitcherAPI;

namespace mmswitcher
{
    public class WindowConditionMonitor : MsgMonitor
    {
        //public WindowConditionMonitor()
        //{

        //}
        protected override bool MessageRecognize(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            throw new NotImplementedException();
        }
    }
}
