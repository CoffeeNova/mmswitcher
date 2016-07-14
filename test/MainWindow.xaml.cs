using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using mmswitcherAPI;
using mmswitcherAPI.winmsg;
using mmswitcherAPI.AltTabSimulator;
using System.Windows.Automation;
//using mmswitcherAPI.Messangers.Web;
using mmswitcherAPI.Messangers;
using mmswitcherAPI.Messangers.Web;

namespace test
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly int _msgNotify;
        private int m = 0;
        WindowMessagesMonitor _wmm;
        ActiveWindowStack aws;
        AutomationElement ae;
        WebSkype ws;

        public MainWindow()
        {
            InitializeComponent();
            //var focusHandler = new AutomationFocusChangedEventHandler(OnFocusChanged);
            //Automation.AddAutomationFocusChangedEventHandler(focusHandler);
            var chromeProcesses = System.Diagnostics.Process.GetProcessesByName("chrome");

            System.Diagnostics.Process process = null;
            foreach (var cProcess in chromeProcesses)
            {
                if (cProcess.MainWindowHandle != IntPtr.Zero)
                {
                    process = cProcess;
                    break;
                }
            }

            if (process == null)
                return;
            ws = new WebSkype(process);
            ws.GotFocus += ws_GotFocus;
            ws.LostFocus += ws_LostFocus;
            ws.GotNewMessage += ws_GotNewMessage;
            ws.MessagesGone += ws_MessagesGone;
            //ws = null;

        }

        void ws_MessagesGone(IMessenger wss)
        {
            Console.WriteLine("Messages read");
        }

        void ws_GotNewMessage(IMessenger wss)
        {
            Console.WriteLine("New Messages");
        }

        void ws_LostFocus(object sender, EventArgs e)
        {
            Console.WriteLine("Lost focus");
        }

        void ws_GotFocus(object sender, EventArgs e)
        {
            Console.WriteLine("Got focus");
        }

        private void OnFocusChanged(object src, AutomationFocusChangedEventArgs e)
        {
            var focusedElement = src as AutomationElement;
            Console.WriteLine(focusedElement.Current.Name + "   " + focusedElement.Current.HasKeyboardFocus.ToString() + "  " + e.ObjectId);
            //e.ChildId;
            // messengerAE.Current.AutomationId
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            _wmm = new WindowMessagesMonitor();
            _wmm.onMessageTraced += _wmm_onMessageTraced;
            aws = ActiveWindowStack.Instance(this);
            aws.onActiveWindowStackChanged += aws_onActiveWindowStackChanged;
            aws.Start();

        }

        void aws_onActiveWindowStackChanged(StackAction action, IntPtr hWnd)
        {
            //int length = Interop.GetWindowTextLength(hWnd);
            //StringBuilder builder = new StringBuilder(length);
            //Interop.GetWindowText(hWnd, builder, length + 1);
            //Console.WriteLine(builder.ToString() + " " + action.ToString());
            //Console.WriteLine("----------------------------------------------");
            //foreach (IntPtr handle in aws.WindowStack)
            //{
            //    int len = WinApi.GetWindowTextLength(handle);
            //    StringBuilder buid = new StringBuilder(len);
            //    WinApi.GetWindowText(handle, buid, len + 1);
            //    Console.WriteLine(buid.ToString());
            //}
        }


        void _wmm_onMessageTraced(object sender, IntPtr hWnd, ShellEvents shell)
        {
            //Console.WriteLine(m = m + 1);
        }

        public static AutomationElement SkypeFocusAutomationElement(IntPtr hWnd)
        {
            //try
            //{
            //    // find the automation element
            //    return BrowserWindowAutomationElement(hWnd);
            //}

            try
            {
                //при переключении вкладок хрома, или операции SetForegroundWindow окна хрома фокус получает контрол "document", его имя класса "Chrome_RenderWidgetHostHWND"
                //при переключении вкладок этот контрол перерисовывается, поэтому чтобы получить нужный, нам необходимо задать фокус на наш мессенджер
                // find the automation element
                var windowAE = AutomationElement.FromHandle(hWnd);
                return windowAE.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ClassNameProperty, "Chrome_RenderWidgetHostHWND"));
            }
            catch { return null; }
        }
    }
}
