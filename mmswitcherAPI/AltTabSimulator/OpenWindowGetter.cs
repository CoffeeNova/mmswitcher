﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace mmswitcherAPI.AltTabSimulator
{
    /// <summary>Contains functionality to get all the open windows.</summary>
    /// http://www.tcx.be/blog/2006/list-open-windows/
    public static class OpenWindowGetter
    {
        /// <summary>Returns a dictionary that contains the handle and title of all the open windows.</summary>
        /// <returns>A dictionary that contains the handle and title of all the open windows.</returns>
        public static IDictionary<IntPtr, string> GetAltTabWindows()
        {
            IntPtr shellWindow = WinApi.GetShellWindow();
            Dictionary<IntPtr, string> windows = new Dictionary<IntPtr, string>();

            WinApi.EnumWindows(delegate(IntPtr hWnd, int lParam)
            {
                //if (hWnd == shellWindow) return true;
                //if (!IsWindowVisible(hWnd)) return true;

                if (!KeepWindowHandleInAltTabList(hWnd))
                    return true;
                int length = WinApi.GetWindowTextLength(hWnd);
                StringBuilder builder = new StringBuilder(length);
                WinApi.GetWindowText(hWnd, builder, length + 1);

                windows[hWnd] = builder.ToString();
                return true;
            }, 0);

            return windows;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<IntPtr> GetAltTabWindowsHandles()
        {
            IntPtr shellWindow = WinApi.GetShellWindow();
            List<IntPtr> windows = new List<IntPtr>();

            WinApi.EnumWindows(delegate(IntPtr hWnd, int lParam)
            {
                //if (hWnd == shellWindow) return true;
                //if (!IsWindowVisible(hWnd)) return true;

                if (!KeepWindowHandleInAltTabList(hWnd))
                    return true;
                windows.Add(hWnd);
                return true;

            }, 0);

            return windows;
        }
        /// <summary>
        /// Returns a pair that contains the handle and title of the window
        /// </summary>
        /// <param name="hWnd">Window handle</param>
        /// <returns>A pair that contains the handle and title of the window</returns>
        public static KeyValuePair<IntPtr, string> GetWindow(IntPtr hWnd)
        {
            KeyValuePair<IntPtr, string> window = new KeyValuePair<IntPtr, string>(IntPtr.Zero, "");

            if (!KeepWindowHandleInAltTabList(hWnd))
                return window;
            int length = WinApi.GetWindowTextLength(hWnd);
            StringBuilder builder = new StringBuilder(length);
            WinApi.GetWindowText(hWnd, builder, length + 1);

            return new KeyValuePair<IntPtr, string>(hWnd, builder.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public static bool KeepWindowHandleInAltTabList(IntPtr window)
        {
            if (window == WinApi.GetShellWindow() || WinApi.GetWindowTextLength(window) == 0)   //Desktop or without title
                return false;

            if (!IsWindowThreadAlive(window))
                return false;

            //http://stackoverflow.com/questions/210504/enumerate-windows-like-alt-tab-does
            //http://blogs.msdn.com/oldnewthing/archive/2007/10/08/5351207.aspx
            //1. For each visible window, walk up its owner chain until you find the root owner. 
            //2. Then walk back down the visible last active popup chain until you find a visible window.
            //3. If you're back to where you're started, (look for exceptions) then put the window in the Alt+Tab list.
            IntPtr root = WinApi.GetAncestor(window, WinApi.GaFlags.GA_ROOTOWNER);
            if (GetLastVisibleActivePopUpOfWindow(root) == window)
            {
                var wi = new Me.Catx.Native.WindowInfo(window);
                if (wi.ClassName == "Shell_TrayWnd" ||                          //Windows taskbar
                    wi.ClassName == "DV2ControlHost" ||                         //Windows startmenu, if open
                    (wi.ClassName == "Button" && wi.WindowText == "Start") ||   //Windows startmenu-button.
                    wi.ClassName == "MsgrIMEWindowClass" ||                     //Live messenger's notifybox i think
                    wi.ClassName == "SysShadow" ||                              //Live messenger's shadow-hack
                    wi.ClassName.StartsWith("WMP9MediaBarFlyout") ||            //WMP's "now playing" taskbar-toolbar
                    wi.ClassName == "ApplicationFrameWindow")                   //win10 applications like calendar, xbox frame and other
                    return false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// http://stackoverflow.com/questions/210504/enumerate-windows-like-alt-tab-does
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        private static IntPtr GetLastVisibleActivePopUpOfWindow(IntPtr window)
        {

            IntPtr lastPopUp = WinApi.GetLastActivePopup(window);
            if (WinApi.IsWindowVisible(lastPopUp))
                return lastPopUp;
            else if (lastPopUp == window)
                return IntPtr.Zero;
            else
                return GetLastVisibleActivePopUpOfWindow(lastPopUp);
        }

        //Defines if process status is "Suspended"
        private static bool IsWindowThreadAlive(IntPtr hWnd)
        {
            int processId;
            uint threadProcessId = WinApi.GetWindowThreadProcessId(hWnd, out processId);
            try
            {
                var proc = Process.GetProcessById((int)processId);
                var threadCollection = proc.Threads.Cast<ProcessThread>();
                int suspendedThreads = threadCollection.Count((p) => { return p.ThreadState == ThreadState.Wait && p.WaitReason == ThreadWaitReason.Suspended; });
                //if all threads have status Suspended then all process is suspended (i believe)
                var t = threadCollection.Count();
                if (threadCollection.Count() == suspendedThreads)
                    return false;
            }
            catch { return false; }
            return true;
        }

    }
}
