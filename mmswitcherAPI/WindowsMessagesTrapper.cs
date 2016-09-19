using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using mmswitcherAPI.AltTabSimulator;

namespace mmswitcherAPI
{
    /// <summary>
    /// Класс для перехвата сообщений Windows.
    /// <remarks>При создании экземпляра </remarks>
    /// </summary>
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

                if (_instance != null)
                    return;

                _initializing = true;
                VoidDelegate resume = ResumeThread;
                int allThreads;
                int activeThreads;
                ThreadPool.GetAvailableThreads(out allThreads, out activeThreads);
                if (activeThreads > 0)
                    StartInThreadPool(resume);
                else
                    StartInNewThread(resume);

                _pauseEvent.WaitOne(Timeout.Infinite);
                _initializing = false;
            }
            return;
        }

        public static void Stop()
        {
            if (_instance == null) throw new InvalidOperationException("Windows messeges handler not started.");
            if (onWndProc != null) throw new InvalidOperationException("Windows messeges handler still has onWndProc listners.");
            /////////////////////////////////////hookmanager listeners!!!11
            onWndProc = null;
            _instance.Invoke(new MethodInvoker(_instance.EndForm));
        }

        private static void ResumeThread()
        {
            _pauseEvent.Set();
        }

        private static void StartInThreadPool(VoidDelegate del)
        {
            ThreadPool.QueueUserWorkItem((state) => { Thread.CurrentThread.Name = _wmtThreadName; RunForm(del); });
        }

        private static void StartInNewThread(VoidDelegate del)
        {
            var t = new Thread(() => RunForm(del));
            t.SetApartmentState(ApartmentState.STA);
            t.IsBackground = true;
            t.Name = _wmtThreadName;
            t.Start();
        }

        protected override void SetVisibleCore(bool value)
        {
            // Prevent window getting visible
            if (_instance == null) CreateHandle();
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
            var handler = onWndProc;
            bool handled = false;
            if (handler != null)
                handler(m.HWnd, m.Msg, m.WParam, m.LParam, ref handled);
            base.WndProc(ref m);
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                onWndProc = null;
                Dispatcher = null;
                _locker = null;
                _instance.Invoke(new MethodInvoker(() => { base.Dispose(disposing); Application.Exit(); }));
            }
            _disposed = true;

        }

        ~WindowsMessagesTrapper()
        {
            Dispose(false);
        }
        /// <summary>
        /// Экземпляр синглтона <see cref="WindowsMessagesTrapper"/>. При попытке получения пустого экземпляра - создаст новый экземпляр.
        /// </summary>
        public static WindowsMessagesTrapper Instance
        {
            get
            {
                if (!_initializing)
                    WindowsMessagesTrapper.Start();
                return _instance;
            }
            private set { _instance = value; }
        }


        //private AltTabHookManager _athManager;
        public static Dispatcher Dispatcher;
        private static WindowsMessagesTrapper _instance;
        private static object _locker = new object();
        private static VoidDelegate resume;
        private static bool _initializing = false;
        private const string _wmtThreadName = "WindowsMessagesTrapper thread";
        private bool _disposed = false;
    }

}
