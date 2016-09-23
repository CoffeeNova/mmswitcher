using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Automation;
using mmswitcherAPI.Messengers.Web.Browsers;

namespace mmswitcherAPI
{
    internal static class DebugLog
    {
        public static void WriteTabSelected(AutomationElement[] tabArray, BrowserSet browserSet)
        {
            if (tabArray[0] != null)
                Debug.WriteLine(String.Format("Selected tab ({0}): {1}", browserSet.MessengerCaption, tabArray[0].Current.BoundingRectangle));
            if (tabArray[1] != null)
                Debug.WriteLine(String.Format("Previous selected tab ({0}): {1}", browserSet.MessengerCaption, tabArray[1].Current.BoundingRectangle));
        }

        public static void WriteBaseMessengerNewMessages(string caption, int messagesCount)
        {
            Debug.WriteLine(string.Format("Messenger caption: {0}, new messages: {1}", caption, messagesCount));
        }

        public static void WriteBaseMessengerFocused(string caption, bool isFocused)
        {
            string s = isFocused == true ? "got focus" : "lost focus";
            Debug.WriteLine(string.Format("Messenger {0} is {1}", caption, s));
        }

        public static void WriteGlobalBindControllerCreated()
        {
            Debug.WriteLine(string.Format("Instance of the class GlobalBindController created."));
        }

        public static void WriteGlobalBindControllerExecuted(string keyName, bool condition)
        {
            Debug.WriteLine(string.Format("GlobalBindController for {0} key {1}.", keyName, condition == true ? "registred" : "unregistered"));
        }
    }
}
