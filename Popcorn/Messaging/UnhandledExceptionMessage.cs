using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;

namespace Popcorn.Messaging
{
    /// <summary>
    /// Used to transmis a Unhandled exception message
    /// </summary>
    public class UnhandledExceptionMessage : MessageBase
    {
        /// <summary>
        /// Unhandled exception
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Create an instance of <see cref="UnhandledExceptionMessage"/>
        /// </summary>
        /// <param name="exception"></param>
        public UnhandledExceptionMessage(Exception exception)
        {
            Exception = exception;
        }
    }
}
