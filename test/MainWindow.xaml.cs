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
using mmswitcherAPI.Messengers;
using mmswitcherAPI.Messengers.Web;
using mmswitcherAPI.Messengers.Desktop;
using System.Threading;

namespace test
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly int _msgNotify;
        private int m = 0;
        //WindowLifeCycle _wmm;
        ActiveWindowStack aws;
        AutomationElement ae;
        WebSkype ws;
        Telegram telegram;
        Skype skype;

        public MainWindow()
        {
            InitializeComponent();
            //var focusHandler = new AutomationFocusChangedEventHandler(OnFocusChanged);
            //Automation.AddAutomationFocusChangedEventHandler(focusHandler);


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
            //_wmm = new WindowLifeCycle();
            //_wmm.onMessageTraced += _wmm_onMessageTraced;

            object win = this;
            Thread thread = new Thread((state) =>
            {
                //Thread.Sleep(1000);
                //aws.Suspend();
                //aws.Dispose();
                //Thread.Sleep(50000);
                //-------------------------------
                //   var inst = WindowsMessagesTrapper.Instance;
                //    var key = new KeyValuePair<System.Windows.Forms.Keys, GlobalBindController.KeyModifierStuck>(System.Windows.Forms.Keys.A, GlobalBindController.KeyModifierStuck.Alt);
                //    var list = new List<KeyValuePair<GlobalBindController.KeyModifierStuck, Action>>();
                //    list.Add(new KeyValuePair<GlobalBindController.KeyModifierStuck, Action>(key.Value, new Action(() => { Console.WriteLine("LOL"); })));

                //GlobalBindController gbc = new GlobalBindController(key.Key, GlobalBindController.BindMethod.RegisterHotKey, GlobalBindController.HookBehaviour.Replacement, list);
                //gbc.Execute = true;

                //gbc.Dispose();
            });
            thread.Name = "Test Thread";
            thread.Start();

        }

        void aws_onActiveWindowStackChanged(StackAction action, IntPtr hWnd)
        {
            int length = WinApi.GetWindowTextLength(hWnd);
            StringBuilder builder = new StringBuilder(length);
            WinApi.GetWindowText(hWnd, builder, length + 1);
            Console.WriteLine(builder.ToString() + " " + action.ToString());
            Console.WriteLine("----------------------------------------------");
            foreach (IntPtr handle in aws.WindowStack)
            {
                int len = WinApi.GetWindowTextLength(handle);
                StringBuilder buid = new StringBuilder(len);
                WinApi.GetWindowText(handle, buid, len + 1);
                Console.WriteLine(buid.ToString());
            }
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

        private void Window_Initialized(object sender, EventArgs e)
        {

        }
        private MessengerController mC;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            aws = ActiveWindowStack.GetInstance();
            aws.Start();
            mC = MessengerController.GetInstance(aws);
            var key = new KeyValuePair<System.Windows.Forms.Keys, GlobalBindController.KeyModifierStuck>(System.Windows.Forms.Keys.A, GlobalBindController.KeyModifierStuck.Alt);
            //mC.SubScribe(MessengerController.SwitchBy.Queue, key);

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
            ws = MessengerBase.Create<WebSkype>(process);
            ws.GotFocus += ws_GotFocus;
            ws.LostFocus += ws_LostFocus;
            ws.GotNewMessage += ws_GotNewMessage;
            ws.MessageGone += ws_MessagesGone;



            //    var telegramProcesses = System.Diagnostics.Process.GetProcessesByName("telegram");
            //    if (telegramProcesses.Count() > 0)
            //        telegram = Telegram.Instance(telegramProcesses.First());
            //    telegram.GotFocus += ws_GotFocus;
            //    telegram.LostFocus += ws_LostFocus;



            //    telegram.SetForeground();

            //    var skypeProcesses = System.Diagnostics.Process.GetProcessesByName("skype");
            //    if (skypeProcesses.Count() > 0)
            //        skype = Skype.Instance(skypeProcesses.First());
            //}
        }
    }
}
