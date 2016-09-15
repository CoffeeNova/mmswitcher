using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mmswitcherAPI.Messengers.Exceptions
{
    public class MessengerBuildException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessengerBuildException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public MessengerBuildException(string message) : base(message) { }

        public MessengerBuildException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class UserPromotedNotificationAreaException : Exception
    {
        public UserPromotedNotificationAreaException(string message) : base(message) { }

        public UserPromotedNotificationAreaException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class TrayButtonException : Exception
    {
        public TrayButtonException(string message) : base(message) { }

        public TrayButtonException(string message, Exception innerException) : base(message, innerException) { }
    }




}
