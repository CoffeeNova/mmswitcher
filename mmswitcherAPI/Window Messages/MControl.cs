using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mmswitcherAPI.winmsg
{
    internal class MControl : Control, IDisposable
    {
        public delegate IntPtr WndProcDelegate(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled);
        public event WndProcDelegate onWndProc;

        protected override void WndProc(ref Message m)
        {
            Console.WriteLine(m.Msg);

            var handler = onWndProc;
            bool handled = false;
            if (handler != null)
                handler(m.HWnd, m.Msg, m.WParam, m.LParam, ref handled);
            base.WndProc(ref m);
        }
        protected override void Dispose(bool disposing)
        {
            onWndProc = null;
            base.Dispose(disposing);
        }
    }
}
