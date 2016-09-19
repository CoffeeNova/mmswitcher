using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Windows.Automation;
using System.Threading;

using mmswitcherAPI.Extensions;
using mmswitcherAPI.Messengers.Exceptions;


namespace mmswitcherAPI.Messengers.Desktop
{
    public abstract class DesktopMessenger : MessengerBase, IDisposable
    {
        protected DesktopMessenger(Process process)
            : base(process)
        {
            if (process == null)
                throw new ArgumentException();

            _instanceList.Add(this);
            MessagesProcessingTimerTick += DesktopMessenger_MessagesProcessingTimerTick;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="TrayButtonException">Происходит, когда не может найти кнопку мессенджера в трее.</exception>
        public override void SetForeground()
        {
            var isVisible = WinApi.IsWindowVisible(base._windowHandle);
            if (!isVisible)
                RestoreFromTray();
            else
            {
                Tools.RestoreWindow(base._windowHandle);
                WinApi.SetForegroundWindow(base._windowHandle);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="process"></param>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        /// <exception cref="ElementNotAvailableException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TrayButtonException"></exception>
        protected override AutomationElement GetMainAutomationElement(Process process, out IntPtr hWnd)
        {
            hWnd = TryGetMainWindowHandle(process);

            bool isRestored = Tools.RestoreWindow(hWnd);
            AutomationElement messengerElement = null;
            try { messengerElement = AutomationElement.FromHandle(hWnd); }
            catch (Exception ex) { throw new ElementNotAvailableException("Cannot get main window automation element.", ex); }
            finally { if (messengerElement == null) Dispose(true); }

            return messengerElement;
        }

        protected AutomationElement TrayButton()
        {
            var trayButtons = UserPromotedNotificationArea.FindAll(TreeScope.Subtree, Condition.TrueCondition);

            var trayButton = trayButtons.Cast<AutomationElement>().FirstOrDefault(x => x.Current.Name.Contains(TrayButtonName));
            if (trayButton == null)
                throw new TrayButtonException(string.Format("Cannot find {0} in a user promoted notification area. Messenger notify icon should be shown in a notification area.", TrayButtonName));
            return trayButton;
        }

        private IntPtr TryGetMainWindowHandle(Process process)
        {
            process.Refresh();
            var hWnd = GetMainWindowHandle(process);

            //probably hided in a tray, should restore a window from it first
            if (hWnd == IntPtr.Zero)
            {
                RestoreFromTray();
                hWnd = GetMainWindowHandle(process);
                if (hWnd == IntPtr.Zero)
                    throw new InvalidOperationException(string.Format("Cannot get main window handle of {0} process.", process.ProcessName));
            }
            return hWnd;
        }

        private void DesktopMessenger_MessagesProcessingTimerTick(object sender, EventArgs e)
        {
            if (_messagesCounter == null)
                return;
            if (base._process.HasExited || base._process == null)
            {
                Dispose(true);
                return;
            }
            var handle = WinApi.OpenProcess(ProcessSecurityAndAccessRights.PROCESS_VM_READ, false, base._process.Id);
            if (handle == IntPtr.Zero)
            {
                Dispose(true);
                return;
            }
            IntPtr bytesRead;
            var buffer = new byte[_messagesCounter.Size];
            var address = IntPtr.Add(_messagesCounter.Address, _messagesCounter.Offset);
            WinApi.ReadProcessMemory(handle, address, buffer, buffer.Length, out bytesRead);
            var intValue = BitConverter.ToInt32(buffer, 0);
            base.IncomeMessages = intValue;
            WinApi.CloseHandle(handle);
//            Debug.WriteLine(string.Format("Process name: {0}, new messages: {1}", base._process.ProcessName, base.IncomeMessages));
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
                throw new UserPromotedNotificationAreaException("Can't find user promoted notification area. Seems like all icons in notification area are disabled. Or explorer.exe is deactivated. Or it is not a Windows.", ex);
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
            _messagesCounter = GetMessagesCounterData();
            if (_messageProcessingTimer == null)
                _messageProcessingTimer = new Timer(MessagesProcessingTimerCallback, null, 0, 1000);
        }

        protected override void OnMessageProcessingUnSubscribe()
        {
            if (_instanceList.Count > 0 || _messageProcessingTimer == null)
                return;
            _messageProcessingTimer.Dispose();
            _messageProcessingTimer = null;
        }

        protected virtual MemoryVariableData GetMessagesCounterData()
        {
            var data = new MemoryVariableData();
            var mainModuleAddress = base._process.MainModule.BaseAddress;
            var offsetsList = MessagesData.Offsets.TakeWhile(p => p != null).Select(p => Convert.ToInt32(p)).ToList();

            var baseAddress= _process.MainModule.BaseAddress;
            var address = baseAddress;

            for (int i = 0; i < offsetsList.Count - 1; i++)
                address = GetMemoryAddress(address, offsetsList[i]);
           
            data.Address = address;
            data.Offset = offsetsList.Last();
            data.Size = MessagesData.Size;
            return data;
        }

        private IntPtr GetMemoryAddress(IntPtr pointer, int offset)
        {
            var handle = WinApi.OpenProcess(ProcessSecurityAndAccessRights.PROCESS_VM_READ, false, base._process.Id);
            IntPtr bytesRead;
            var buffer = new byte[4];
            pointer = IntPtr.Add(pointer, offset);
            WinApi.ReadProcessMemory(handle, pointer, buffer, buffer.Length, out bytesRead);
            WinApi.CloseHandle(handle);
            return (IntPtr)BitConverter.ToInt32(buffer, 0);
        }
        #region abstract methods
        //protected abstract void _wm_paintMonitor_onMessageTraced(object sender, IntPtr hWnd, ShellEvents shell);
        //protected abstract MemoryVariableData GetMessagesCounterData();

        protected abstract IntPtr GetMainWindowHandle(Process process);

        protected abstract bool RestoreFromTray();
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                _messagesCounter = null;
                MessagesProcessingTimerTick -= DesktopMessenger_MessagesProcessingTimerTick;
                _instanceList.Remove(this);
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
        protected static AutomationElement UserPromotedNotificationArea
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

        protected abstract MessagesVariableLocation MessagesData { get; }
        /// <summary>
        /// Счетчик сообщений в памяти процесса мессенджера.
        /// </summary>
        #endregion

        #region private fields
        //private MessengerHookManager _hManager;
        //private IntPtr _notifyIconHwnd;
        private static AutomationElement _userPromotedNotificationArea = GetNotificationArea();
        private static Timer _messageProcessingTimer;
        private bool _disposed = false;
        private MemoryVariableData _messagesCounter;
        private static List<DesktopMessenger> _instanceList = new List<DesktopMessenger>();
        #endregion

        protected static event EventHandler MessagesProcessingTimerTick;
    }

    public class MemoryVariableData
    {
        public int Size { get; set; }
        public IntPtr Address { get; set; }
        public int Offset { get; set; }
    }

    public struct MessagesVariableLocation
    {
        public int MaxPointerLevel;
        public int PointerOffset;
        public int?[] Offsets;
        public int Size;
    }
}