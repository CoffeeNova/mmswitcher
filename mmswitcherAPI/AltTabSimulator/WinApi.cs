using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace mmswitcherAPI.AltTabSimulator
{
    internal class WinApi
    {
        internal delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

        [DllImport("USER32.DLL")]
        internal static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("USER32.DLL")]
        internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        internal static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        internal static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        internal static extern IntPtr GetShellWindow();

        [DllImport("user32.dll", ExactSpelling = true)]
        internal static extern IntPtr GetAncestor(IntPtr hwnd, GaFlags flags);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetLastActivePopup(IntPtr hWnd);

        internal delegate void WinEventHookProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime);

        [DllImport("Kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetExitCodeThread(IntPtr hThread, out uint lpExitCode);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int ProcessId);

        [DllImport("Kernel32.dll")]
        internal static extern IntPtr GetThreadId(IntPtr hWnd);

        [DllImport("Kernel32.dll")]
        internal static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [System.Runtime.ConstrainedExecution.ReliabilityContract(System.Runtime.ConstrainedExecution.Consistency.WillNotCorruptState, System.Runtime.ConstrainedExecution.Cer.Success)]
        [System.Security.SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);
        /// <summary>
        /// Sets an event hook function for a range of events.
        /// </summary>
        /// <param name="eventMin"> 
        /// Type: UINT
        /// Specifies the event constant for the lowest event value in the range of events that are handled by the hook function. This parameter can be set to EVENT_MIN to indicate the lowest possible event value.
        /// </param>
        /// <param name="eventMax">
        /// Type: UINT
        /// Specifies the event constant for the highest event value in the range of events that are handled by the hook function. This parameter can be set to EVENT_MAX to indicate the highest possible event value.
        /// </param>
        /// <param name="hmodWinEventProc">
        /// Type: HMODULE
        /// Handle to the DLL that contains the hook function at lpfnWinEventProc, if the WINEVENT_INCONTEXT flag is specified in the dwFlags parameter. If the hook function is not located in a DLL, or if the WINEVENT_OUTOFCONTEXT flag is specified, this parameter is NULL.
        /// </param>
        /// <param name="lpfnWinEventProc">
        /// Type: WINEVENTPROC
        /// Pointer to the event hook function. For more information about this function, see WinEventProc.
        /// </param>
        /// <param name="idProcess">
        /// Type: DWORD
        /// Specifies the ID of the process from which the hook function receives events. Specify zero (0) to receive events from all processes on the current desktop.
        /// </param>
        /// <param name="idThread">
        /// Type: DWORD
        /// Specifies the ID of the thread from which the hook function receives events. If this parameter is zero, the hook function is associated with all existing threads on the current desktop.
        /// </param>
        /// <param name="dwflags">
        /// Type: UINT
        /// Flag values that specify the location of the hook function and of the events to be skipped.
        /// </param>
        /// <returns>
        /// Type: HWINEVENTHOOK
        /// If successful, returns an HWINEVENTHOOK value that identifies this event hook instance. Applications save this return value to use it with the UnhookWinEvent function.
        /// If unsuccessful, returns zero.
        /// </returns>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int SetWinEventHook(int eventMin, int eventMax, IntPtr hmodWinEventProc, WinEventHookProc lpfnWinEventProc, int idProcess, int idThread, int dwflags);

        /// <summary>
        /// Removes an event hook function created by a previous call to SetWinEventHook.
        /// </summary>
        /// <param name="hWinEventHook">
        /// Type: HWINEVENTHOOK
        /// Handle to the event hook returned in the previous call to SetWinEventHook.
        /// </param>
        /// <returns>
        /// Type: BOOL
        /// If successful, returns TRUE; otherwise, returns FALSE.
        /// </returns>
        [DllImport("user32.dll")]
        internal static extern bool UnhookWinEvent(int hWinEventHook);

        internal enum GaFlags
        {
            /// <summary>
            /// Retrieves the parent window. This does not include the owner, as it does with the GetParent function. 
            /// </summary>
            GA_PARENT = 1,
            /// <summary>
            /// Retrieves the root window by walking the chain of parent windows.
            /// </summary>
            GA_ROOT = 2,
            /// <summary>
            /// Retrieves the owned root window by walking the chain of parent and owner windows returned by GetParent. 
            /// </summary>
            GA_ROOTOWNER = 3
        }

        [Flags]
        internal enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }
    }
}
