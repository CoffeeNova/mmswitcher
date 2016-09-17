using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using mmswitcherAPI.Messengers.Exceptions;

namespace Click_
{
    internal static class ExceptionHandler
    {
        public static void Handle(MessengerBuildException ex, string messengerType, bool closeApplication)
        {
            if (ex.InnerException.GetType() == typeof(UserPromotedNotificationAreaException))
            {
                _log.Error("Can't get UserPromotedNotificationArea.");
                closeApplication = true;
            }
                
            if (ex.InnerException.GetType() == typeof(TrayButtonException))
            {
                _log.Error(string.Format("Can't get a tray button of {0}. Be sure that {0} messenger tray button has status as \"always show in the notification area\".", messengerType));
                closeApplication = true;
            }
            else
                _log.Info(string.Format("Expectation of start of a {0}.", messengerType));
            Close(closeApplication);
        }

        public static void Handle(InvalidOperationException ex, bool closeApplication)
        {
            _log.Error(ex.Message);
            Close(closeApplication);
        }

        public static void Handle(InvalidOperationException ex, Exception innerException, bool closeApplication)
        {
            _log.Error(string.Format("{0}: {1}", ex.Message, innerException.Message));
            Close(closeApplication);
        }

        private static void Close(bool close)
        {
            if (!close)
                return;
            _log.Info(string.Format("{0} is closed with error.", Constants._APPLICATION_NAME));
            Application.Exit();
        }
        private static Logger _log = LogManager.GetCurrentClassLogger();
    }
}
