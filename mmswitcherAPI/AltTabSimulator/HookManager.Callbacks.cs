using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace mmswitcherAPI.AltTabSimulator
{
    public static partial class HookManager
    {
        private static int s_ForegroundChangedHookHandle;
        private static WinApi.WinEventHookProc s_ForegroundChangedDelegate;
        private static void ForegroundChangedHookProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
#if DEBUG
            Console.WriteLine(string.Format("hWinEventHook: {0}, iEvent: {1}, hWnd: {2},  idObject: {3}, idChild: {4}, dwEventThread: {5}, dwmsEventTime:{6}", hWinEventHook, iEvent, hWnd, idObject, idChild, dwEventThread, dwmsEventTime));
#endif
            try
            {
                EventArgs e = new EventArgs();
                s_ForegroundChanged.Invoke(hWnd, e);
            }
            catch (Exception ex)
            { Console.WriteLine(ex.Message); }
        }

        private static void EnsureSubscribedToForegroundChangedEvent()
        {
            // install Focus hook only if it is not installed and must be installed
            if (s_ForegroundChangedHookHandle == 0)
            {

                //See comment of this field. To avoid GC to clean it up.
                s_ForegroundChangedDelegate = ForegroundChangedHookProc;
                //install hook
                s_ForegroundChangedHookHandle = WinApi.SetWinEventHook(EventConstants.EVENT_SYSTEM_FOREGROUND, EventConstants.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, s_ForegroundChangedDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);

                //If SetWinEventHook fails.
                if (s_ForegroundChangedHookHandle == 0)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    int errorCode = Marshal.GetLastWin32Error();
                    //do cleanup

                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
            }

        }
        private static void TryUnsubscribeFromForegroundChangedEvent()
        {
            //if no subsribers are registered unsubsribe from hook
            if (s_ForegroundChanged == null)
            {
                ForceUnsunscribeFromForegroundChangedEvent();
            }
        }

        private static void ForceUnsunscribeFromForegroundChangedEvent()
        {
            if (s_ForegroundChangedHookHandle != 0)
            {
                //uninstall hook
                bool result = WinApi.UnhookWinEvent(s_ForegroundChangedHookHandle);
                //reset invalid handle
                s_ForegroundChangedHookHandle = 0;
                //Free up for GC
                s_ForegroundChangedDelegate = null;
                //if failed and exception must be thrown
                if (result == false)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    int errorCode = Marshal.GetLastWin32Error();
                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
            }
        }

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/dd318066(v=vs.85).aspx
        /// </summary>
        private struct EventConstants
        {
            public const int EVENT_OBJECT_CREATE = 0x8000;
            public const int EVENT_OBJECT_SHOW = 0x8002;
            public const int EVENT_OBJECT_REORDER = 0x8004;
            public const int EVENT_OBJECT_FOCUS = 0x8005;
            public const int EVENT_OBJECT_CONTENTSCROLLED = 0x8015;
            public const int EVENT_OBJECT_SELECTION = 0x8006;
            public const int EVENT_OBJECT_SELECTIONREMOVE = 0x8008;
            public const int EVENT_OBJECT_SELECTIONWITHIN = 0x8009;
            public const int EVENT_SYSTEM_FOREGROUND = 0x0003;
            public const int EVENT_OBJECT_SELECTIONADD = 0x0007;
            public const int EVENT_OBJECT_STATECHANGE = 0x800A;
            public const int EVENT_OBJECT_NAMECHANGE = 0x800C;
        }
        /// <summary>
        /// The callback function is not mapped into the address space of the process that generates the event. Because the hook function is called across process boundaries, the system must queue events. Although this method is asynchronous, events are guaranteed to be in sequential order.
        /// </summary>
        private const int WINEVENT_OUTOFCONTEXT = 0;
    }
}
