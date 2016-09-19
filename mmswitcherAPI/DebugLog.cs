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

        public static void WriteNewMessages(string caption, int messagesCount)
        {
            Debug.WriteLine(string.Format("Messenger caption: {0}, new messages: {1}", caption, messagesCount));
        }
    }
}
