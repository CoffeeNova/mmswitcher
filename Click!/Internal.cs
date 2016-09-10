using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWTweak;
using System.Windows.Forms;
using mmswitcherAPI;

namespace Click_
{
    using Gbc = GlobalBindController;

    internal class Internal
    {
        internal static void SaveRegistrySettings(ref bool lastValue, string valueName, string keyLocation, bool saveValue)
        {
            if (lastValue == saveValue)
                return;
            if (RegistryWorker.WriteKeyValue(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, Microsoft.Win32.RegistryValueKind.String, valueName, saveValue == true ? "true" : "false"))
                lastValue = saveValue;
            else
                throw new InvalidOperationException("UNABLE TO USE REGKEY HKEY_LOCAL_MACHINE\\" + keyLocation + "\\" + valueName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lastValue"></param>
        /// <param name="valueName"></param>
        /// <param name="keyLocation"></param>
        /// <param name="saveValue"></param>
        internal static void SaveRegistrySettings(ref Keys lastValue, string valueName, string keyLocation, Keys saveValue)
        {
            if (lastValue == saveValue)
                return;
            if (RegistryWorker.WriteKeyValue(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, Microsoft.Win32.RegistryValueKind.String, valueName, saveValue.ToString()))
                lastValue = saveValue;
            else
                throw new InvalidOperationException("UNABLE TO USE REGKEY HKEY_LOCAL_MACHINE\\" + keyLocation + "\\" + valueName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lastValue"></param>
        /// <param name="keyName"></param>
        /// <param name="keyLocation"></param>
        /// <param name="saveValue"></param>
        internal static void SaveRegistrySettings(ref List<Gbc.KeyModifierStuck> lastValue, string valueName, string keyLocation, List<Gbc.KeyModifierStuck> saveValue)
        {
            if (lastValue == saveValue)
                return;
            if (RegistryWorker.WriteKeyValue(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, Microsoft.Win32.RegistryValueKind.MultiString, valueName, saveValue.Select(x => { return x.ToString(); }).ToArray()))
                lastValue = saveValue;
            else
                throw new InvalidOperationException("UNABLE TO USE REGKEY HKEY_LOCAL_MACHINE\\" + keyLocation + "\\" + valueName);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValue"></param>
        /// <param name="valueName"></param>
        /// <param name="keyLocation"></param>
        /// <param name="keyDefaultValue"></param>
        internal static void CheckRegistrySettings(ref string keyValue, string valueName, string keyLocation, string keyDefaultValue)
        {
            if (RegistryWorker.GetKeyValue<string>(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, valueName) != null)
                keyValue = RegistryWorker.GetKeyValue<string>(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, valueName);
            else if (RegistryWorker.WriteKeyValue(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, Microsoft.Win32.RegistryValueKind.String, valueName, keyDefaultValue))
                keyValue = keyDefaultValue;
            else
                throw new InvalidOperationException("UNABLE TO USE REGKEY HKEY_LOCAL_MACHINE\\" + keyLocation + "\\" + valueName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValue"></param>
        /// <param name="valueName"></param>
        /// <param name="keyLocation"></param>
        /// <param name="keyDefaultValue"></param>
        internal static void CheckRegistrySettings(ref bool keyValue, string valueName, string keyLocation, bool keyDefaultValue)
        {
            if (RegistryWorker.GetKeyValue<string>(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, valueName) != null)
                keyValue = RegistryWorker.GetKeyValue<string>(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, valueName) == "true" ? true : false;
            else if (RegistryWorker.WriteKeyValue(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, Microsoft.Win32.RegistryValueKind.String, valueName, keyDefaultValue == true ? "true" : "false"))
                keyValue = keyDefaultValue;
            else
                throw new InvalidOperationException("UNABLE TO USE REGKEY HKEY_LOCAL_MACHINE\\" + keyLocation + "\\" + valueName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValue"></param>
        /// <param name="valueName"></param>
        /// <param name="keyLocation"></param>
        /// <param name="keyDefaultValue"></param>
        /// <returns></returns>
        internal static bool CheckRegistrySettings(string valueName, string keyLocation, bool keyDefaultValue)
        {
            bool keyValue;
            if (RegistryWorker.GetKeyValue<string>(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, valueName) != null)
                keyValue = RegistryWorker.GetKeyValue<string>(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, valueName) == "true" ? true : false;
            else if (RegistryWorker.WriteKeyValue(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, Microsoft.Win32.RegistryValueKind.String, valueName, keyDefaultValue == true ? "true" : "false"))
                keyValue = keyDefaultValue;
            else
                throw new InvalidOperationException("UNABLE TO USE REGKEY HKEY_LOCAL_MACHINE\\" + keyLocation + "\\" + valueName);
            return keyValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValue"></param>
        /// <param name="valueName"></param>
        /// <param name="keyLocation"></param>
        /// <param name="keyDefaultValue"></param>
        internal static void CheckRegistrySettings(ref Keys keyValue, string valueName, string keyLocation, Keys keyDefaultValue)
        {
            try
            {
                if (RegistryWorker.GetKeyValue<string>(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, valueName) != null)
                    keyValue = ConvertFromString(RegistryWorker.GetKeyValue<string>(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, valueName));
                else if (RegistryWorker.WriteKeyValue(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, Microsoft.Win32.RegistryValueKind.String, valueName, keyDefaultValue.ToString()))
                    keyValue = keyDefaultValue;
                else
                    throw new InvalidOperationException("UNABLE TO USE REGKEY HKEY_LOCAL_MACHINE\\" + keyLocation + "\\" + valueName);
            }
            catch
            {
                throw new InvalidOperationException("WRONG VALUE AT HKEY_LOCAL_MACHINE\\" + keyLocation + "\\" + valueName);
            }
        }

        internal static Keys CheckRegistrySettings(string valueName, string keyLocation, Keys keyDefaultValue)
        {
            Keys keyValue;
            try
            {
                if (RegistryWorker.GetKeyValue<string>(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, valueName) != null)
                    keyValue = ConvertFromString(RegistryWorker.GetKeyValue<string>(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, valueName));
                else if (RegistryWorker.WriteKeyValue(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, Microsoft.Win32.RegistryValueKind.String, valueName, keyDefaultValue.ToString()))
                    keyValue = keyDefaultValue;
                else
                    throw new InvalidOperationException("UNABLE TO USE REGKEY HKEY_LOCAL_MACHINE\\" + keyLocation + "\\" + valueName);
            }
            catch
            {
                throw new InvalidOperationException("WRONG VALUE AT HKEY_LOCAL_MACHINE\\" + keyLocation + "\\" + valueName);
            }
            return keyValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValue"></param>
        /// <param name="valueName"></param>
        /// <param name="keyLocation"></param>
        internal static void CheckRegistrySettings(ref List<Gbc.KeyModifierStuck> keyValue, string valueName, string keyLocation, List<Gbc.KeyModifierStuck> keyDefaultValue)
        {
            Func<List<Gbc.KeyModifierStuck>, string[]> fu = (gbc_list) =>
            {
                string[] test = new string[gbc_list.Count];
                for (int i = 0; i < gbc_list.Count; i++)
                    test[i] = gbc_list[i].ToString();
                return test;
            };

            if (RegistryWorker.GetKeyValue<string[]>(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, valueName) != null)
            {
                string[] keyValueReg = RegistryWorker.GetKeyValue<string[]>(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, valueName);
                Func<string, Gbc.KeyModifierStuck> func = (value) =>
                {
                    Gbc.KeyModifierStuck mod;
                    #region switch-case shit
                    switch (value)
                    {
                        case "Shift":
                            mod = Gbc.KeyModifierStuck.Shift;
                            break;
                        case "Control":
                            mod = Gbc.KeyModifierStuck.Control;
                            break;
                        case "Alt":
                            mod = Gbc.KeyModifierStuck.Alt;
                            break;
                        case "Winkey":
                            mod = Gbc.KeyModifierStuck.WinKey;
                            break;
                        case "ShiftAlt":
                            mod = Gbc.KeyModifierStuck.ShiftAlt;
                            break;
                        case "ShiftControl":
                            mod = Gbc.KeyModifierStuck.ShiftControl;
                            break;
                        case "ShiftControlALt":
                            mod = Gbc.KeyModifierStuck.ShiftControlAlt;
                            break;
                        case "None":
                            mod = Gbc.KeyModifierStuck.None;
                            break;
                        case "ControlAlt":
                            mod = Gbc.KeyModifierStuck.ControlAlt;
                            break;
                        default:
                            mod = Gbc.KeyModifierStuck.None;
                            break;
                    }
                    #endregion
                    return mod;

                };
                keyValue.Clear();
                foreach (string keyV in keyValueReg)
                    keyValue.Add(func(keyV));
            }
            else if (RegistryWorker.WriteKeyValue(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, Microsoft.Win32.RegistryValueKind.MultiString, valueName, fu(keyDefaultValue)))
                keyValue = keyDefaultValue;
            else
                throw new InvalidOperationException("UNABLE TO USE REGKEY HKEY_LOCAL_MACHINE\\" + keyLocation + "\\" + valueName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keystr"></param>
        /// <returns></returns>
        internal static Keys ConvertFromString(string keystr)
        {
            return (Keys)Enum.Parse(typeof(Keys), keystr);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gbc"></param>
        /// <returns></returns>
        internal static string ConvertFromGBC(Gbc.KeyModifierStuck gbc)
        {
            return gbc.ToString();
        }
    }
}
