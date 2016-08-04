using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace mmswitcherAPI.Extensions
{
    public static class AuomationElementExtension
    {
        public static bool IsAlive(this AutomationElement element)
        {
            try
            {
                var name = element.Current.Name;
            }
            catch(ElementNotAvailableException)
            {
                return false;
            }
            return true;
        }
    }
}
