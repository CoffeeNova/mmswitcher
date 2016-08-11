using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mmswitcherAPI.Messengers;
using System.Diagnostics;
using System.Windows.Automation;
using mmswitcherAPI.Extensions;
using System.Threading;
using mmswitcherAPI.winmsg;
using System.Runtime.InteropServices;

namespace mmswitcherAPI.Messengers.Desktop
{
    public abstract class DesktopMessenger : MessengerBase, IDisposable
    {
        protected DesktopMessenger(Process process)
            : base(process)
        {
            if (process == null)
                throw new ArgumentException();
        }

        public override void SetForeground()
        {
            //throw new NotImplementedException();
        }

        protected override AutomationElement GetMainAutomationElement(Process process, out IntPtr hWnd)
        {
            hWnd = TryGetMainWindowHandle(process);

            bool isRestored = Tools.RestoreWindow(hWnd);
            AutomationElement messengerElement = null;
            try { messengerElement = AutomationElement.FromHandle(hWnd); }
            catch (Exception ex) { throw new Exception("Cannot get main window automation element.", ex); }
            finally { if (messengerElement == null) Dispose(true); }

            return messengerElement;
        }

        protected override AutomationElement GetFocusRecieverAutomationElement(IntPtr hWnd)
        {
            if (hWnd == null)
                throw new ArgumentNullException("hWnd");
            if (hWnd == IntPtr.Zero)
                throw new ArgumentException("Window handle should not be IntPtr.Zero");

            WINDOWPLACEMENT placement = Tools.GetPlacement(hWnd);
            var state = placement.showCmd;
            if (state == ShowWindowCommands.Minimized || state == ShowWindowCommands.Hide)
                RestoreFromTray();
            return FocusReciever(hWnd);

        }

        protected override AutomationElement GetIncomeMessageAutomationElement(IntPtr hWnd)
        {
            return UserPromotedNotificationArea;
            //return base.GetIncomeMessageAutomationElement(hWnd);
        }

        



        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }


        private void DefineNotifyHandle()
        {

        }

        private bool RestoreFromTray()
        {
            var trayButton = TrayButton();
            
            if ((bool)trayButton.GetCurrentPropertyValue(AutomationElement.IsInvokePatternAvailableProperty))
            {
                var pattern = (InvokePattern)trayButton.GetCurrentPattern(InvokePattern.Pattern);
                pattern.Invoke();
            }
            else
                Tools.SimulateClickUIAutomation(trayButton, UserPromotedNotificationArea, (IntPtr)UserPromotedNotificationArea.Current.NativeWindowHandle);
            return true;
        }

        private AutomationElement TrayButton()
        {
            var trayButton = UserPromotedNotificationArea.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, TrayButtonName));
            if (trayButton == null)
                throw new Exception(string.Format("Cannot find {0} in a user promoted notification area. Messenger notify icon should be shown in a notification area.", TrayButtonName));
            return trayButton;
        }

        private IntPtr TryGetMainWindowHandle(Process process)
        {
            var hWnd = process.MainWindowHandle;

            //probably hided in a tray, should restore a window from it first
            if (hWnd == IntPtr.Zero)
            {
                RestoreFromTray();
                Thread.Sleep(100);
                hWnd = process.MainWindowHandle;
                if (hWnd == IntPtr.Zero)
                    throw new Exception(string.Format("Cannot get main window handle of {0} process.", process.ProcessName));
            }
            return hWnd;
        }

        private static AutomationElement GetNotificationArea()
        {
            try
            {
                var root = AutomationElement.RootElement;
                var shellTrayAE = root.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ClassNameProperty, "Shell_TrayWnd"));
                var trayNotifyAE = shellTrayAE.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ClassNameProperty, "TrayNotifyWnd"));
                var sysPagerAE = trayNotifyAE.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ClassNameProperty, "SysPager"));
                var toolbarAE = sysPagerAE.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ClassNameProperty, "ToolbarWindow32"));
                if (toolbarAE == null)
                    throw new Exception();
                return toolbarAE;
            }
            catch (Exception ex)
            {
                throw new Exception("Can't find user promoted notification area. Seems like all icons in notification area are disabled. Or explorer.exe is deactivated. Or it is not a Windows.", ex);
            }
        }

        private static AutomationElement TryGetNotificationArea()
        {
            try { return GetNotificationArea(); }
            catch { return null; }
        }

        private static void MessagesProcessingTimerCallback(object state)
        {
            MessagesProcessingTimerTick.Invoke(state, new EventArgs());
        }

        protected override void OnMessageProcessingSubscribe()
        {
            if (_messageProcessingTimer == null)
                _messageProcessingTimer = new Timer(MessagesProcessingTimerCallback, null, 1000, 1000);
        }

        #region abstract methods
        protected abstract AutomationElement FocusReciever(IntPtr hWnd);
        protected abstract void _wm_paintMonitor_onMessageTraced(object sender, IntPtr hWnd, ShellEvents shell);
        #endregion

        #region public properties
        public override Messenger Messenger
        {
            get { throw new NotImplementedException(); }
        }
        #endregion

        #region private properties
        private static AutomationElement UserPromotedNotificationArea
        {
            get
            {
                if (_userPromotedNotificationArea.IsAlive())
                    return _userPromotedNotificationArea;
                else
                    return TryGetNotificationArea();
            }
        }
        #endregion
        #region protected properties
        protected abstract string TrayButtonName { get; }
        #endregion

        #region private fields
        private MessengerHookManager hManager;
        private IntPtr _notifyIconHwnd;
        private static AutomationElement _userPromotedNotificationArea = GetNotificationArea();
        private static Timer _messageProcessingTimer;
        #endregion

        protected static event EventHandler MessagesProcessingTimerTick;
    }
}


//var windowHandle = (IntPtr)IncomeMessageAE.Current.NativeWindowHandle;
//var processId = (IntPtr)IncomeMessageAE.Current.ProcessId;
//uint threadID = WinApi.GetWindowThreadProcessId(windowHandle, processId);
//IntPtr hMod = WinApi.LoadLibrary(@"e:\Visual Studio Projects\mmswitcher\mmswitcherAPI\bin\Debug\mmswitcherAPI.dll");
//IntPtr hMod = WinApi.LoadLibrary(@"c:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\Tools\spyxxhk_amd64.dll");
//IntPtr hMod = Marshal.GetHINSTANCE(GetType().Module);
//var asm = System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0];
//hMod = Marshal.GetHINSTANCE(asm);
//hMod = WinApi.LoadLibrary("user32.dll");
//hMod = WinApi.LoadLibrary(@"e:\Visual Studio Projects\mmswitcher\mmswitcherAPI\bin\Debug\mmswitcherAPI.dll");
//hMod = Marshal.GetHINSTANCE(typeof(DesktopMessenger).Module);
//int processId;
//uint threadId = WinApi.GetWindowThreadProcessId((IntPtr)IncomeMessageAE.Current.NativeWindowHandle, out processId);
//testMon = new WM_PAINT_Monitor1(GlobalHookTypes.AfterWindow, hMod, 0);
////testMon = new WM_PAINT_Monitor(GlobalHookTypes.AfterWindow, IntPtr.Zero, IntPtr.Zero);

//testMon = new WM_PAINT_Monitor((IntPtr)IncomeMessageAE.Current.NativeWindowHandle);
//var hManager = new MessengerHookManager((IntPtr)IncomeMessageAE.Current.NativeWindowHandle);
//hManager.EventsListener += hManager_EventsListener;