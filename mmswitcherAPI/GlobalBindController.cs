using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace mmswitcherAPI
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GlobalBindController : NativeWindow, IDisposable
    {

        public GlobalBindController(Keys bindKey, BindMethod method, HookBehaviour behaviour, List<KeyValuePair<KeyModifierStuck, Action>> modifiers)
        {
            this.CreateHandle(new CreateParams());
            _key = bindKey;
            Method = method;
            Behaviour = behaviour;
            _ptrHookKey = IntPtr.Zero;
            _ptrHookModKey = IntPtr.Zero;
            _shiftDetected = false;
            _controlDetected = false;
            _altDetected = false;
            _tasks = modifiers;
            _bindFunctions = CreateBindActions(_tasks);
            _functionSuccessful = false;
            DebugLog.WriteGlobalBindControllerCreated();
        }

        private BindFunctions CreateBindActions(List<KeyValuePair<KeyModifierStuck, Action>> modPairs)
        {
            BindFunctions bf = new BindFunctions();
            foreach (KeyValuePair<KeyModifierStuck, Action> mod in modPairs)
            {
                switch (mod.Key)
                {
                    case KeyModifierStuck.None:
                        bf.Nomod = mod.Value;
                        break;
                    case KeyModifierStuck.Shift:
                        bf.Shiftmod = mod.Value;
                        break;
                    case KeyModifierStuck.Control:
                        bf.Controlmod = mod.Value;
                        break;
                    case KeyModifierStuck.Alt:
                        bf.Altmod = mod.Value;
                        break;
                    case KeyModifierStuck.WinKey:
                        bf.Winmod = mod.Value;
                        break;
                    case KeyModifierStuck.ShiftControl:
                        bf.CtrlShiftmod = mod.Value;
                        break;
                    case KeyModifierStuck.ShiftAlt:
                        bf.ShiftAltmod = mod.Value;
                        break;
                    case KeyModifierStuck.ControlAlt:
                        bf.CtrlAltmod = mod.Value;
                        break;
                    case KeyModifierStuck.ShiftControlAlt:
                        bf.ShiftCtrlAltmod = mod.Value;
                        break;
                }
            }
            return bf;
        }

        private void ModifierRegister(Keys key)
        {
            if (key == Keys.LShiftKey || key == Keys.RShiftKey)
                _shiftDetected = true;
            if (key == Keys.LControlKey || key == Keys.RControlKey)
                _controlDetected = true;
            if (key == Keys.LMenu || key == Keys.RMenu)
                _altDetected = true;
            if (key == Keys.LWin || key == Keys.RWin)
                _winDetected = true;
        }

        private void ModifierUnregister(Keys key)
        {
            if (key == Keys.LShiftKey || key == Keys.RShiftKey)
                _shiftDetected = false;
            if (key == Keys.LControlKey || key == Keys.RControlKey)
                _controlDetected = false;
            if (key == Keys.LMenu || key == Keys.RMenu)
                _altDetected = false;
            if (key == Keys.LWin || key == Keys.RWin)
                _winDetected = false;
        }
        //
        internal bool DoBindFunction(Keys key)
        {
            if (_onDelay)
                return true;
            //Условие 1: клавиша без модификатора.
            if (key == _key && !_shiftDetected && !_controlDetected && !_altDetected && !_winDetected && _bindFunctions.Nomod != null)
                _bindFunctions.Nomod();
            //Условие 2: клавиша с модификатором shift.
            else if (key == _key && _shiftDetected && !_controlDetected && !_altDetected && !_winDetected && _bindFunctions.Shiftmod != null)
                _bindFunctions.Shiftmod();
            //Условие 3: клавиша с модификатором ctrl.
            else if (key == _key && !_shiftDetected && _controlDetected && !_altDetected && !_winDetected && _bindFunctions.Controlmod != null)
                _bindFunctions.Controlmod();
            //Условие 4: клавиша с модификатором alt.
            else if (key == _key && !_shiftDetected && !_controlDetected && _altDetected && !_winDetected && _bindFunctions.Altmod != null)
                _bindFunctions.Altmod();
            //Условие 5: клавиша с модификатором win.
            else if (key == _key && !_shiftDetected && !_controlDetected && !_altDetected && _winDetected && _bindFunctions.Winmod != null)
                _bindFunctions.Winmod();
            //Условие 6: клавиша с модификатором ctrl+shift.
            else if (key == _key && _shiftDetected && _controlDetected && !_altDetected && !_winDetected && _bindFunctions.CtrlShiftmod != null)
                _bindFunctions.CtrlShiftmod();
            //Условие 7: клавиша с модификатором ctrl+alt.
            else if (key == _key && !_shiftDetected && _controlDetected && _altDetected && !_winDetected && _bindFunctions.CtrlAltmod != null)
                _bindFunctions.CtrlAltmod();
            //Условие 8: клавиша с модификатором shift+alt.
            else if (key == _key && _shiftDetected && !_controlDetected && _altDetected && !_winDetected && _bindFunctions.ShiftAltmod != null)
                _bindFunctions.ShiftAltmod();
            //Условие 9: клавиша с модификатором shift+ctrl+alt.
            else if (key == _key && _shiftDetected && _controlDetected && _altDetected && !_winDetected && _bindFunctions.ShiftCtrlAltmod != null)
                _bindFunctions.ShiftCtrlAltmod();
            else
                return false;
            Delay();
            return true;
        }

        private void BindFunction(bool state)
        {
            #region Switch hook method
            if (state && Method == BindMethod.Hook && PtrHookKey == IntPtr.Zero)
            {
                ProcessModule objCurrentModule = Process.GetCurrentProcess().MainModule;
                //ProcessModule objExplorerModule = Process.GetProcessesByName("explorer")[0].MainModule;
                //modifier
                _modifierKeyboardProcess = new WinApi.HookProc((int nCode, IntPtr wp, IntPtr lp) =>
                {
                    KBDLLHOOKSTRUCT objKeyInfo = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lp, typeof(KBDLLHOOKSTRUCT));
                    if (nCode >= 0 && ((Int32)wp == Constants.WM_KEYDOWN || (Int32)wp == Constants.WM_SYSKEYDOWN))
                        ModifierRegister(objKeyInfo.key);

                    if (nCode >= 0 && (Int32)wp == Constants.WM_KEYUP)
                    {
                        ModifierUnregister(objKeyInfo.key);
                        return _functionSuccessful == true ? WinApi.CallNextHookEx(_ptrHookModKey, 0, wp, lp) : WinApi.CallNextHookEx(_ptrHookModKey, nCode, wp, lp);
                    }
                    return WinApi.CallNextHookEx(_ptrHookModKey, nCode, wp, lp);
                });
                _ptrHookModKey = WinApi.SetWindowsHookEx(13, _modifierKeyboardProcess, WinApi.GetModuleHandle(objCurrentModule.ModuleName), 0);

                //key
                _keyKeyboardProcess = new WinApi.HookProc((int nCode, IntPtr wp, IntPtr lp) =>
                {
                    KBDLLHOOKSTRUCT objKeyInfo = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lp, typeof(KBDLLHOOKSTRUCT));
                    if (nCode >= 0 && (Int32)wp == Constants.WM_KEYDOWN)
                        if (DoBindFunction(objKeyInfo.key) && Behaviour == HookBehaviour.Replacement)
                        {
                            _functionSuccessful = true;
                            return (IntPtr)1;
                        }
                    _functionSuccessful = false;
                    return WinApi.CallNextHookEx(_ptrHookKey, nCode, wp, lp);
                });
                _ptrHookKey = WinApi.SetWindowsHookEx(13, _keyKeyboardProcess, WinApi.GetModuleHandle(objCurrentModule.ModuleName), 0);
            }
            else if (!state && Method == BindMethod.Hook)
            {
                if (_ptrHookKey != IntPtr.Zero)
                {
                    WinApi.UnhookWindowsHookEx(_ptrHookKey);
                    _ptrHookKey = IntPtr.Zero;
                }
                if (_ptrHookModKey != IntPtr.Zero)
                {
                    WinApi.UnhookWindowsHookEx(_ptrHookModKey);
                    _ptrHookModKey = IntPtr.Zero;
                }
            }
            #endregion
            #region CapsSwitch RegisterHotKey method

            if (state && Method == BindMethod.RegisterHotKey && !_keyRegistred)
            {
                if (_bindFunctions.Shiftmod != null || _bindFunctions.ShiftAltmod != null || _bindFunctions.CtrlShiftmod != null || _bindFunctions.ShiftCtrlAltmod != null)
                    _shiftKeyRegistred = WinApi.RegisterHotKey(this.Handle, Constants.SHIFTKEY_REGISTER_ID, (int)KeyModifier.Shift, _key.GetHashCode());
                if (_bindFunctions.Controlmod != null || _bindFunctions.CtrlShiftmod != null || _bindFunctions.CtrlAltmod != null || _bindFunctions.ShiftCtrlAltmod != null)
                    _controlKeyRegistred = WinApi.RegisterHotKey(this.Handle, Constants.CTRLKEY_REGISTER_ID, (int)KeyModifier.Control, _key.GetHashCode());
                if (_bindFunctions.Altmod != null || _bindFunctions.CtrlAltmod != null || _bindFunctions.ShiftAltmod != null || _bindFunctions.ShiftCtrlAltmod != null)
                    _altKeyRegistred = WinApi.RegisterHotKey(this.Handle, Constants.ALTKEY_REGISTER_ID, (int)KeyModifier.Alt, _key.GetHashCode());
                if (_bindFunctions.Winmod != null)
                    _winKeyRegistred = WinApi.RegisterHotKey(this.Handle, Constants.WINKEY_REGISTER_ID, (int)KeyModifier.WinKey, _key.GetHashCode());
                if (_bindFunctions.Nomod != null)
                    _keyRegistred = WinApi.RegisterHotKey(this.Handle, Constants.KEY_REGISTER_ID, (int)KeyModifier.None, _key.GetHashCode());
            }
            else if (!state && Method == BindMethod.RegisterHotKey)
            {
                if (_keyRegistred)
                    _keyRegistred = !WinApi.UnregisterHotKey(this.Handle, Constants.KEY_REGISTER_ID);
                if (_shiftKeyRegistred)
                    _shiftKeyRegistred = !WinApi.UnregisterHotKey(this.Handle, Constants.SHIFTKEY_REGISTER_ID);
                if (_controlKeyRegistred)
                    _controlKeyRegistred = !WinApi.UnregisterHotKey(this.Handle, Constants.CTRLKEY_REGISTER_ID);
                if (_altKeyRegistred)
                    _altKeyRegistred = !WinApi.UnregisterHotKey(this.Handle, Constants.ALTKEY_REGISTER_ID);
                if (_winKeyRegistred)
                    _winKeyRegistred = !WinApi.UnregisterHotKey(this.Handle, Constants.WINKEY_REGISTER_ID);
            }
            #endregion
        }

        private async void Delay()
        {
            _onDelay = true;
            await System.Threading.Tasks.Task.Delay(KeyProcessingDelay);
            _onDelay = false;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Constants.WM_HOTKEY && _onDelay == false)
            {
                /* Note that the three lines below are not needed if you only want to register one hotkey.
                 * The below lines are useful in case you want to register multiple keys, which you can use a switch with the id as argument, or if you want to know which key/modifier was pressed for some particular reason. */

                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);                  // The key of the hotkey that was pressed.
                KeyModifier modifier = (KeyModifier)((int)m.LParam & 0xFFFF);       // The modifier of the hotkey that was pressed.
                int id = m.WParam.ToInt32();                                        // The id of the hotkey that was pressed.

                if (modifier == KeyModifier.None && _bindFunctions.Nomod != null)
                    _bindFunctions.Nomod();
                else if (modifier == KeyModifier.Shift && _bindFunctions.Shiftmod != null)
                    _bindFunctions.Shiftmod();
                else if (modifier == KeyModifier.Control && _bindFunctions.Controlmod != null)
                    _bindFunctions.Controlmod();
                else if (modifier == KeyModifier.Alt && _bindFunctions.Altmod != null)
                    _bindFunctions.Altmod();
                else if (modifier == KeyModifier.WinKey && _bindFunctions.Winmod != null)
                    _bindFunctions.Winmod();
                else if (modifier == (KeyModifier.Shift | KeyModifier.Control) && _bindFunctions.CtrlShiftmod != null)
                    _bindFunctions.CtrlShiftmod();
                else if (modifier == (KeyModifier.Shift | KeyModifier.Alt) && _bindFunctions.ShiftAltmod != null)
                    _bindFunctions.ShiftAltmod();
                else if (modifier == (KeyModifier.Control | KeyModifier.Alt) && _bindFunctions.CtrlAltmod != null)
                    _bindFunctions.CtrlAltmod();
                Delay();
            }
            base.WndProc(ref m);

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                BindFunction(false);
                _modifierKeyboardProcess = null;
                _keyKeyboardProcess = null;
                _tasks = null;
                Execute = false;
                this.DestroyHandle();
            }

            disposed = true;
        }
        ~GlobalBindController() { Dispose(false); }

        #region private fields
        private bool disposed = false;
        private bool _shiftDetected;
        private bool _controlDetected;
        private bool _altDetected;
        private bool _winDetected;
        private bool _keyRegistred;
        private bool _shiftKeyRegistred;
        private bool _controlKeyRegistred;
        private bool _altKeyRegistred;
        private bool _winKeyRegistred;
        private bool _functionSuccessful;
        private bool _execute = false;
        private bool _onDelay = false;
        private Keys _key;
        private WinApi.HookProc _modifierKeyboardProcess;
        private WinApi.HookProc _keyKeyboardProcess;
        private IntPtr _ptrHookModKey;
        private IntPtr _ptrHookKey;
        private BindFunctions _bindFunctions;
        private List<KeyValuePair<KeyModifierStuck, Action>> _tasks;
        private int _keyProcessingDelay = 0;
        private static readonly int _maxDelay = 2000;
        private static readonly int _minDelay = 0;
        #endregion

        #region public properties

        public bool Execute
        {
            get { return _execute; }
            set
            {
                if (value != _execute)
                {
                    BindFunction(value);
                    DebugLog.WriteGlobalBindControllerExecuted(_key.ToString(), value);
                    _execute = value;
                }
            }
        }
        public BindMethod Method { get; private set; }
        public HookBehaviour Behaviour { get; private set; }
        public IntPtr PtrHookKey { get { return _ptrHookKey; } }
        public IntPtr PtrHookModKey { get { return _ptrHookModKey; } }
        public Keys Key
        {
            get { return _key; }
            set
            {
                if (value != _key && _execute)
                {
                    Execute = false;
                    _key = value;
                    Execute = true;
                    return;
                }
                _key = value;
            }
        }
        public List<KeyValuePair<KeyModifierStuck, Action>> Tasks
        {
            get { return _tasks; }
            set
            {
                if (value != _tasks && _execute)
                {
                    Execute = false;
                    _tasks = value;
                    _bindFunctions = CreateBindActions(_tasks);
                    Execute = true;
                    return;
                }
                _tasks = value;
            }
        }

        public int KeyProcessingDelay
        {
            get { return _keyProcessingDelay; }
            set
            {
                if (value > _maxDelay || value < _minDelay)
                    throw new ArgumentOutOfRangeException("KeyProcessingDelay", string.Format("Must be within the range from {0} to {1}", _minDelay, _maxDelay));
                _keyProcessingDelay = value;
            }
        }

        #endregion

        #region enums

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

        public enum KeyModifierStuck
        {
            None,
            Shift,
            Control,
            Alt,
            WinKey,
            ShiftControl,
            ShiftAlt,
            ControlAlt,
            ShiftControlAlt
        }

        private enum KeyModifier
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            WinKey = 8
        }

        #endregion

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

    }
}
