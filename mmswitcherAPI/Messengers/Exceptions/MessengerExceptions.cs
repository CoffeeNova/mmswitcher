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
}
