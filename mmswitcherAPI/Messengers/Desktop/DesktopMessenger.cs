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

            _messagesCounter = GetMessagesCounterData();
            MessagesProcessingTimerTick += DesktopMessenger_MessagesProcessingTimerTick;
        }

        public override void SetForeground()
        {
            var isVisible = WinApi.ShowWindow(base._windowHandle, ShowWindowEnum.Show);
            if (!isVisible)
                RestoreFromTray();
            //  Thread.Sleep(100);
            else
                FocusableAE.SetFocus();
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


        private void DefineNotifyHandle()
        {

        }

        protected bool RestoreFromTray()
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
            var trayButtons = UserPromotedNotificationArea.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button));
            var trayButton = trayButtons.Cast<AutomationElement>().First(x => x.Current.Name.Contains(TrayButtonName));
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
                var isVisible = WinApi.ShowWindow(hWnd, ShowWindowEnum.Show);
                if (!isVisible)
                {
                    RestoreFromTray();
                    Thread.Sleep(100);
                }
                hWnd = process.MainWindowHandle;
                if (hWnd == IntPtr.Zero)
                    throw new Exception(string.Format("Cannot get main window handle of {0} process.", process.ProcessName));
            }
            return hWnd;
        }

        private void DesktopMessenger_MessagesProcessingTimerTick(object sender, EventArgs e)
        {
            if(_messagesCounter ==null)
                return;
            if (base._process.HasExited)
                Dispose(true);
            var handle = WinApi.OpenProcess(ProcessSecurityAndAccessRights.PROCESS_VM_READ, false, base._process.Id);
            if (handle == IntPtr.Zero)
                Dispose(true);
            IntPtr bytesRead;
            var buffer = new byte[_messagesCounter.Size];
            var address = IntPtr.Add(_messagesCounter.Address, _messagesCounter.Offset);
            WinApi.ReadProcessMemory(handle, address, buffer, buffer.Length, out bytesRead);
            IncomeMessages = BitConverter.ToInt32(buffer, 0);
            WinApi.CloseHandle(handle);

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
            if (MessagesProcessingTimerTick == null)
                return;
            MessagesProcessingTimerTick.Invoke(state, new EventArgs());
        }

        protected override void OnMessageProcessingSubscribe()
        {
            if (_messageProcessingTimer == null)
                _messageProcessingTimer = new Timer(MessagesProcessingTimerCallback, null, 3000, 1000);
        }

        protected override void OnMessageProcessingUnSubscribe()
        {
            _messageProcessingTimer.Dispose();
        }
        #region abstract methods
        //protected abstract void _wm_paintMonitor_onMessageTraced(object sender, IntPtr hWnd, ShellEvents shell);
        protected abstract VariableData GetMessagesCounterData();
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                _messagesCounter = null;
                _userPromotedNotificationArea = null;
            }
            _disposed = true;
            base.Dispose(disposing);
        }

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

        /// <summary>
        /// Счетчик сообщений в памяти процесса мессенджера.
        /// </summary>
        #endregion

        #region overrided properties
        protected override int IncomeMessages { get; set; }
        #endregion

        #region private fields
        //private MessengerHookManager _hManager;
        //private IntPtr _notifyIconHwnd;
        private static AutomationElement _userPromotedNotificationArea = GetNotificationArea();
        private static Timer _messageProcessingTimer;
        private bool _disposed = false;
        private VariableData _messagesCounter;
        #endregion

        protected static event EventHandler MessagesProcessingTimerTick;
    }

    public class VariableData
    {
        public int Size { get; set; }
        public IntPtr Address { get; set; }
        public int Offset { get; set; }
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