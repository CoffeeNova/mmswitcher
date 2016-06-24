using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Gma.UserActivityMonitor;
using mmswitcherAPI;
using mmswitcherAPI.winmsg;

namespace mmswitcherAPI.AltTabSimulator
{
    //http://csharpindepth.com/articles/general/singleton.aspx - about singltons

    /// <summary>
    /// Класс реализует функционал, аналогичный (максимально приближенный) работе с переключением окон сочетанием клавиш alt+tab
    /// </summary>
    public sealed class AltTabSimulator
    {
        private WindowMessagesMonitor _winMesMon;
        private static readonly Lazy<AltTabSimulator> _instance = new Lazy<AltTabSimulator>(() => new AltTabSimulator());

        public static AltTabSimulator Instance
        {
            get { return _instance.Value; }
        }

        private List<IntPtr> _altTabList;
        public List<IntPtr> AltTabList
        {
            get { return _altTabList; }
        }

        private bool started = false;
        public bool Started
        {
            get { return started; }
        }

        private  bool suspended = true;
        public  bool Suspended
        {
            get { return suspended; }
        }

        private AltTabSimulator() 
        {
            _winMesMon = new WindowMessagesMonitor()

        }

        public void Start()
        {
            if (!started && suspended)
            {
                RefreshAltTabList();
                systemProcessHook.WindowEvent += systemProcessHook_WindowEvent;
                HookManager.ForegroundChanged += HookManager_ForegroundChanged;
                started = true;
                suspended = false;
            }
        }

        public void Suspend()
        {
            if (!suspended && started)
            {
                ClearAltTabList();
                systemProcessHook.WindowEvent -= systemProcessHook_WindowEvent;
                HookManager.ForegroundChanged -= HookManager_ForegroundChanged;
                suspended = true;
                started = false;
            }
        }
        /// <summary>
        ///Обновляет список видимых окон, как в списке альт таба (почти как, еще добавлет закрытые окна, окторые в вин10 почему-то остаются висеть в процессах, типа calc.exe)
        /// </summary>
        private void RefreshAltTabList()
        {
            _altTabList = OpenWindowGetter.GetAltTabWindowsHandles(); 
        }
        private void ClearAltTabList()
        {
            _altTabList.Clear();
        }
        private void HookManager_ForegroundChanged(object sender, EventArgs e)
        {
            bool newWindow = true;
            IntPtr fore = (IntPtr)sender;

            try
            {
                // try to find new foreground window in alt tab list
                foreach (IntPtr hWnd in _altTabList)
                    if (hWnd == fore)
                    {
                        newWindow = false;
                        break;
                    }
                //Thread.Sleep(10);
                //if (newWindow && OpenWindowGetter.KeepWindowHandleInAltTabList(fore))
                //{
                //    altTabList.Insert(0, fore);
                //    Console.WriteLine(WowDisableWinKeyTools.GetWindowTitle(fore) + " " + fore.ToString());
                //}
                if (!newWindow)
                {
                    IntPtr windowHWnd = _altTabList.Find(x => x == fore);
                    if (windowHWnd != IntPtr.Zero)
                    {
                        _altTabList.Remove(windowHWnd);
                        _altTabList.Insert(0, windowHWnd);
                        _tabOrWindow = SwitchTo.Window;
                    }
                }
                //check if window exists, remove from list if not

            }
            catch { }

        }

        private void systemProcessHook_WindowEvent(object sender, IntPtr handle, Interop.ShellEvents shell)
        {
            if (shell == Interop.ShellEvents.HSHELL_WINDOWDESTROYED)
                if (_altTabList.Remove(handle))
                    Console.WriteLine("hwnd:{0}, title:{1} removed. altTabList cound ={2}", handle.ToString(), WowDisableWinKeyTools.GetWindowTitle(handle), _altTabList.Count.ToString());
            if (shell == Interop.ShellEvents.HSHELL_WINDOWCREATED && OpenWindowGetter.KeepWindowHandleInAltTabList(handle))
            {
                _altTabList.Insert(0, handle);
                Console.WriteLine("hwnd:{0}, title:{1} inserted. altTabList cound ={2}", handle.ToString(), WowDisableWinKeyTools.GetWindowTitle(handle), _altTabList.Count.ToString());
                _tabOrWindow = SwitchTo.Window;
            }
        }
        private void Dispose()
        {
            try
            {
                systemProcessHook.WindowEvent -= systemProcessHook_WindowEvent;
                HookManager.ForegroundChanged -= HookManager_ForegroundChanged;
            }
            catch { }
        }
        ~AltTabSimulator()
        {
            Dispose();
        }
    }
    public enum SwitchTo
    {
        Tab = 0,
        Window = 1
    }

}
