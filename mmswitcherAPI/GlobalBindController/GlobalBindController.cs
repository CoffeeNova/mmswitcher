using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace mmswitcherAPI.Gbc
{
    public class GlobalBindController : Control, IDisposable
    {
        //delegate void ChangeKeyboradLayoutDelegate(LanguageLayout lanLaym, Locks locks);

        public enum BindMethod
        {
            Hook,
            RegisterHotKey
        }
        public enum HookBehaviour
        {
            Replacement = 0,
            Joint = 1
        }

        private enum KeyModifier
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            WinKey = 8
        }

        private bool shiftDetected;
        private bool controlDetected;
        private bool altDetected;
        private bool winDetected;
        private bool keyRegistred;
        private bool shiftKeyRegistred;
        private bool controlKeyRegistred;
        private bool altKeyRegistred;
        private bool winKeyRegistred;
        private bool functionSuccessful;
        private LowLevelKeyboardProc modifierKeyboardProcess;
        private LowLevelKeyboardProc keyKeyboardProcess;
        private IntPtr ptrHookModKey;
        private IntPtr ptrHookKey;
        private BindFunctions bindFunctions;

        private bool execute = false;
        public bool Execute
        {
            get { return execute; }
            set
            {
                if (value != execute)
                {
                    BindFunction(value);
                    execute = value;
                }
            }
        }
        public BindMethod Method { get; private set; }
        public HookBehaviour Behaviour { get; private set; }
        public IntPtr PtrHookKey { get { return ptrHookKey; } }
        public IntPtr PtrHookModKey { get { return ptrHookModKey; } }
        private Keys key;
        public Keys Key
        {
            get { return key; }
            set
            {
                if (value != key && execute)
                {
                    key = value;
                    Execute = false;
                    Execute = true;
                    return;
                }
                key = value;
            }
        }
        private List<KeyValuePair<GBC_KeyModifier, Action>> tasks;
        public List<KeyValuePair<GBC_KeyModifier, Action>> Tasks
        {
            get { return tasks; }
            set
            {
                if (value != tasks && execute)
                {
                    tasks = value;
                    Execute = false;
                    bindFunctions = CreateBindActions(tasks);
                    Execute = true;
                    return;
                }
                tasks = value;
            }
        }

        private struct BindFunctions
        {
            public Action Nomod;
            public Action Shiftmod;
            public Action Controlmod;
            public Action Altmod;
            public Action Winmod;
            public Action CtrlShiftmod;
            public Action CtrlAltmod;
            public Action ShiftAltmod;
            public Action ShiftCtrlAltmod;
        }


        public enum Locks : ushort
        {
            None = 0,
            KeyboardScrollLockOn = 1,
            KeyboardNumLockOn = 2,
            KeyboardCapsLockOn = 4
        }

        public GlobalBindController(Keys bindKey, BindMethod method, HookBehaviour behaviour, List<KeyValuePair<GBC_KeyModifier, Action>> modifiers)
        {
            key = bindKey;
            Method = method;
            Behaviour = behaviour;
            Execute = false;
            ptrHookKey = IntPtr.Zero;
            ptrHookModKey = IntPtr.Zero;
            shiftDetected = false;
            controlDetected = false;
            altDetected = false;
            tasks = modifiers;
            bindFunctions = CreateBindActions(tasks);

            functionSuccessful = false;
        }
        private BindFunctions CreateBindActions(List<KeyValuePair<GBC_KeyModifier, Action>> modPairs)
        {
            BindFunctions bf = new BindFunctions();
            foreach (KeyValuePair<GBC_KeyModifier, Action> mod in modPairs)
            {
                switch (mod.Key)
                {
                    case GBC_KeyModifier.None:
                        bf.Nomod = mod.Value;
                        break;
                    case GBC_KeyModifier.Shift:
                        bf.Shiftmod = mod.Value;
                        break;
                    case GBC_KeyModifier.Control:
                        bf.Controlmod = mod.Value;
                        break;
                    case GBC_KeyModifier.Alt:
                        bf.Altmod = mod.Value;
                        break;
                    case GBC_KeyModifier.WinKey:
                        bf.Winmod = mod.Value;
                        break;
                    case GBC_KeyModifier.ShiftControl:
                        bf.CtrlShiftmod = mod.Value;
                        break;
                    case GBC_KeyModifier.ShiftAlt:
                        bf.ShiftAltmod = mod.Value;
                        break;
                    case GBC_KeyModifier.ControlAlt:
                        bf.CtrlAltmod = mod.Value;
                        break;
                    case GBC_KeyModifier.ShiftControlAlt:
                        bf.ShiftCtrlAltmod = mod.Value;
                        break;
                }
            }
            return bf;
        }

        private void ModifierRegister(Keys key)
        {
            if (key == Keys.LShiftKey || key == Keys.RShiftKey)
                shiftDetected = true;
            if (key == Keys.LControlKey || key == Keys.RControlKey)
                controlDetected = true;
            if (key == Keys.LMenu || key == Keys.RMenu)
                altDetected = true;
            if (key == Keys.LWin || key == Keys.RWin)
                winDetected = true;
        }

        private void ModifierUnregister(Keys key)
        {
            if (key == Keys.LShiftKey || key == Keys.RShiftKey)
                shiftDetected = false;
            if (key == Keys.LControlKey || key == Keys.RControlKey)
                controlDetected = false;
            if (key == Keys.LMenu || key == Keys.RMenu)
                altDetected = false;
            if (key == Keys.LWin || key == Keys.RWin)
                winDetected = false;
        }
        //
        internal bool DoBindFunction(Keys _key)
        {
            //Условие 1: клавиша без модификатора.
            if (_key == key && !shiftDetected && !controlDetected && !altDetected && !winDetected && bindFunctions.Nomod != null)
            {
                bindFunctions.Nomod();
                return true;
            }
            //Условие 2: клавиша с модификатором shift.
            if (_key == key && shiftDetected && !controlDetected && !altDetected && !winDetected && bindFunctions.Shiftmod != null)
            {
                bindFunctions.Shiftmod();
                return true;
            }
            //Условие 3: клавиша с модификатором ctrl.
            if (_key == key && !shiftDetected && controlDetected && !altDetected && !winDetected && bindFunctions.Controlmod != null)
            {
                bindFunctions.Controlmod();
                return true;
            }
            //Условие 4: клавиша с модификатором alt.
            if (_key == key && !shiftDetected && !controlDetected && altDetected && !winDetected && bindFunctions.Altmod != null)
            {
                bindFunctions.Altmod();
                return true;
            }
            //Условие 5: клавиша с модификатором win.
            if (_key == key && !shiftDetected && !controlDetected && !altDetected && winDetected && bindFunctions.Winmod != null)
            {
                bindFunctions.Winmod();
                return true;
            }
            //Условие 6: клавиша с модификатором ctrl+shift.
            if (_key == key && shiftDetected && controlDetected && !altDetected && !winDetected && bindFunctions.CtrlShiftmod != null)
            {
                bindFunctions.CtrlShiftmod();
                return true;
            }
            //Условие 7: клавиша с модификатором ctrl+alt.
            if (_key == key && !shiftDetected && controlDetected && altDetected && !winDetected && bindFunctions.CtrlAltmod != null)
            {
                bindFunctions.CtrlAltmod();
                return true;
            }
            //Условие 8: клавиша с модификатором shift+alt.
            if (_key == key && shiftDetected && !controlDetected && altDetected && !winDetected && bindFunctions.ShiftAltmod != null)
            {
                bindFunctions.ShiftAltmod();
                return true;
            }
            //Условие 9: клавиша с модификатором shift+ctrl+alt.
            if (_key == key && shiftDetected && controlDetected && altDetected && !winDetected && bindFunctions.ShiftCtrlAltmod != null)
            {
                bindFunctions.ShiftCtrlAltmod();
                return true;
            }
            return false;
        }

        private void BindFunction(bool state)
        {
            #region Switch hook method
            if (state && Method == BindMethod.Hook && PtrHookKey == IntPtr.Zero)
            {
                ProcessModule objCurrentModule = Process.GetCurrentProcess().MainModule;
                //ProcessModule objExplorerModule = Process.GetProcessesByName("explorer")[0].MainModule;
                //modifier
                modifierKeyboardProcess = new LowLevelKeyboardProc((int nCode, IntPtr wp, IntPtr lp) =>
                {
                    KBDLLHOOKSTRUCT objKeyInfo = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lp, typeof(KBDLLHOOKSTRUCT));
                    if (nCode >= 0 && ((Int32)wp == (int)WindowMessage.WM_KEYDOWN || (Int32)wp == (int)(int)WindowMessage.WM_SYSKEYDOWN))
                        ModifierRegister(objKeyInfo.key);

                    if (nCode >= 0 && (Int32)wp == (int)WindowMessage.WM_KEYUP)
                    {
                        ModifierUnregister(objKeyInfo.key);
                        return functionSuccessful == true ? WinApi.CallNextHookEx(ptrHookModKey, 0, wp, lp) : WinApi.CallNextHookEx(ptrHookModKey, nCode, wp, lp);
                    }
                    return WinApi.CallNextHookEx(ptrHookModKey, nCode, wp, lp);
                });
                ptrHookModKey = WinApi.SetWindowsHookEx(13, modifierKeyboardProcess, WinApi.GetModuleHandle(objCurrentModule.ModuleName), 0);

                //key
                keyKeyboardProcess = new LowLevelKeyboardProc((int nCode, IntPtr wp, IntPtr lp) =>
                {
                    KBDLLHOOKSTRUCT objKeyInfo = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lp, typeof(KBDLLHOOKSTRUCT));
                    if (nCode >= 0 && (Int32)wp == (int)WindowMessage.WM_KEYDOWN)
                        if (DoBindFunction(objKeyInfo.key) && Behaviour == HookBehaviour.Replacement)
                        {
                            functionSuccessful = true;
                            return (IntPtr)1;
                        }
                    functionSuccessful = false;
                    return WinApi.CallNextHookEx(ptrHookKey, nCode, wp, lp);
                });
                ptrHookKey = WinApi.SetWindowsHookEx(13, keyKeyboardProcess, WinApi.GetModuleHandle(objCurrentModule.ModuleName), 0);
            }
            else if (!state || Method == BindMethod.Hook)
            {
                if (ptrHookKey != IntPtr.Zero)
                {
                    WinApi.UnhookWindowsHookEx(ptrHookKey);
                    ptrHookKey = IntPtr.Zero;
                }
                if (ptrHookModKey != IntPtr.Zero)
                {
                    WinApi.UnhookWindowsHookEx(ptrHookModKey);
                    ptrHookModKey = IntPtr.Zero;
                }
            }
            #endregion
            #region CapsSwitch RegisterHotKey method

            if (state && Method == BindMethod.RegisterHotKey && !keyRegistred)
            {
                if (bindFunctions.Shiftmod != null || bindFunctions.ShiftAltmod != null || bindFunctions.CtrlShiftmod != null || bindFunctions.ShiftCtrlAltmod != null)
                    shiftKeyRegistred = WinApi.RegisterHotKey(this.Handle, Constants.SHIFTKEY_REGISTER_ID, (int)KeyModifier.Shift, key.GetHashCode());
                if (bindFunctions.Controlmod != null || bindFunctions.CtrlShiftmod != null || bindFunctions.CtrlAltmod != null || bindFunctions.ShiftCtrlAltmod != null)
                    controlKeyRegistred = WinApi.RegisterHotKey(this.Handle, Constants.CTRLKEY_REGISTER_ID, (int)KeyModifier.Control, key.GetHashCode());
                if (bindFunctions.Altmod != null || bindFunctions.CtrlAltmod != null || bindFunctions.ShiftAltmod != null || bindFunctions.ShiftCtrlAltmod != null)
                    altKeyRegistred = WinApi.RegisterHotKey(this.Handle, Constants.ALTKEY_REGISTER_ID, (int)KeyModifier.Alt, key.GetHashCode());
                if (bindFunctions.Winmod != null)
                    winKeyRegistred = WinApi.RegisterHotKey(this.Handle, Constants.WINKEY_REGISTER_ID, (int)KeyModifier.WinKey, key.GetHashCode());
                if (bindFunctions.Nomod != null)
                    keyRegistred = WinApi.RegisterHotKey(this.Handle, Constants.KEY_REGISTER_ID, (int)KeyModifier.None, key.GetHashCode());

            }
            else if (!state && Method == BindMethod.RegisterHotKey)
            {
                if (keyRegistred)
                {
                    WinApi.UnregisterHotKey(this.Handle, Constants.KEY_REGISTER_ID);
                    keyRegistred = false;
                }
                if (shiftKeyRegistred)
                {
                    WinApi.UnregisterHotKey(this.Handle, Constants.SHIFTKEY_REGISTER_ID);
                    shiftKeyRegistred = false;
                }
                if (controlKeyRegistred)
                {
                    WinApi.UnregisterHotKey(this.Handle, Constants.CTRLKEY_REGISTER_ID);
                    controlKeyRegistred = false;
                }
                if (altKeyRegistred)
                {
                    WinApi.UnregisterHotKey(this.Handle, Constants.ALTKEY_REGISTER_ID);
                    altKeyRegistred = false;
                }
                if (winKeyRegistred)
                {
                    WinApi.UnregisterHotKey(this.Handle, Constants.WINKEY_REGISTER_ID);
                    winKeyRegistred = false;
                }
            }
            #endregion

        }


        protected override void WndProc(ref Message m)
        {
            if (m.Msg == (int)WindowMessage.WM_HOTKEY)
            {
                /* Note that the three lines below are not needed if you only want to register one hotkey.
                 * The below lines are useful in case you want to register multiple keys, which you can use a switch with the id as argument, or if you want to know which key/modifier was pressed for some particular reason. */

                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);                  // The key of the hotkey that was pressed.
                KeyModifier modifier = (KeyModifier)((int)m.LParam & 0xFFFF);       // The modifier of the hotkey that was pressed.
                int id = m.WParam.ToInt32();                                        // The id of the hotkey that was pressed.

                if (modifier == KeyModifier.None && bindFunctions.Nomod != null)
                    bindFunctions.Nomod();
                if (modifier == KeyModifier.Shift && bindFunctions.Shiftmod != null)
                    bindFunctions.Shiftmod();
                if (modifier == KeyModifier.Control && bindFunctions.Controlmod != null)
                    bindFunctions.Controlmod();
                if (modifier == KeyModifier.Alt && bindFunctions.Altmod != null)
                    bindFunctions.Altmod();
                if (modifier == KeyModifier.WinKey && bindFunctions.Winmod != null)
                    bindFunctions.Winmod();
                if (modifier == (KeyModifier.Shift | KeyModifier.Control) && bindFunctions.CtrlShiftmod != null)
                    bindFunctions.CtrlShiftmod();
                if (modifier == (KeyModifier.Shift | KeyModifier.Alt) && bindFunctions.ShiftAltmod != null)
                    bindFunctions.ShiftAltmod();
                if (modifier == (KeyModifier.Control | KeyModifier.Alt) && bindFunctions.CtrlAltmod != null)
                    bindFunctions.CtrlAltmod();
            }
            base.WndProc(ref m);

        }

        private bool disposed = false;

        //public void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Manual release of managed resources.
                }

                if (keyRegistred)
                    WinApi.UnregisterHotKey(this.Handle, Constants.KEY_REGISTER_ID);
                if (shiftKeyRegistred)
                    WinApi.UnregisterHotKey(this.Handle, Constants.SHIFTKEY_REGISTER_ID);
                if (controlKeyRegistred)
                    WinApi.UnregisterHotKey(this.Handle, Constants.CTRLKEY_REGISTER_ID);
                if (altKeyRegistred)
                    WinApi.UnregisterHotKey(this.Handle, Constants.ALTKEY_REGISTER_ID);
                if (winKeyRegistred)
                    WinApi.UnregisterHotKey(this.Handle, Constants.WINKEY_REGISTER_ID);
                disposed = true;

                base.Dispose();
            }
        }
        ~GlobalBindController() { Dispose(false); }

    }
}