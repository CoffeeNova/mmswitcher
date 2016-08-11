using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;

namespace mmswitcherAPI.winmsg
{
    internal class WindowsMessagesTrapper : Form
    {
        public delegate IntPtr WndProcDelegate(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled);
        public static event WndProcDelegate onWndProc;
        private delegate void VoidDelegate();
        private static ManualResetEvent _pauseEvent = new ManualResetEvent(false);

        private delegate Task formStartedDelegate();

        private static void RunForm(VoidDelegate vd)
        {
            resume = vd;
            WindowsMessagesTrapper.Dispatcher = Dispatcher.CurrentDispatcher;
            Application.Run(new WindowsMessagesTrapper());
        }

        private void EndForm()
        {
            this.Close();
        }

        public static void Start()
        {
            lock (_locker)
            {

                if (Instance != null)
                    return;
                VoidDelegate resume = ResumeThread;
                Thread t = new Thread(() => RunForm(resume));
                t.SetApartmentState(ApartmentState.STA);
                t.IsBackground = true;
                t.Start();
                _pauseEvent.WaitOne(Timeout.Infinite);
            }
            return;
        }

        public static void Stop()
        {
            if (Instance == null) throw new InvalidOperationException("Windows messeges handler not started.");
            if (onWndProc != null) throw new InvalidOperationException("Windows messeges handler still has onWndProc listners.");
            onWndProc = null;
            Instance.Invoke(new MethodInvoker(Instance.EndForm));
        }

        private static void ResumeThread()
        {
            _pauseEvent.Set();
        }

        protected override void SetVisibleCore(bool value)
        {
            // Prevent window getting visible
            if (Instance == null) CreateHandle();
            Instance = this;
            value = false;
            base.SetVisibleCore(value);
            resume.Invoke();
        }

        protected override void OnActivated(EventArgs e)
        {
           
            base.OnActivated(e);
        }


        protected override void WndProc(ref Message m)
        {
            Debug.WriteLine(m.Msg);
            var handler = onWndProc;
            bool handled = false;
            if (handler != null)
                handler(m.HWnd, m.Msg, m.WParam, m.LParam, ref handled);
            base.WndProc(ref m);
        }

        protected override void Dispose(bool disposing)
        {
            onWndProc = null;
            Dispatcher = null;
            _locker = null;
            Instance.Invoke(new MethodInvoker(Instance.EndForm));
            base.Dispose(disposing);
        }

        public static WindowsMessagesTrapper Instance;
        public static Dispatcher Dispatcher;
        private static object _locker = new object();
        private static VoidDelegate resume;

    }

}
