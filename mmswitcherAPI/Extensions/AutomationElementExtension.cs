using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace mmswitcherAPI.Extensions
{
    internal static class AutomationElementExtension
    {
        public static bool IsAlive(this AutomationElement element)
        {
            try
            {
                var name = element.Current.Name;
            }
            catch (ElementNotAvailableException)
            {
                return false;
            }
            return true;
        }

        public static bool IsAlive(this List<AutomationElement> element)
        {
            try
            {
                string name;
                foreach (var e in element)
                    name = e.Current.Name;
            }
            catch (ElementNotAvailableException)
            {
                return false;
            }
            return true;
        }
    }
}
