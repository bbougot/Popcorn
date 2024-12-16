using System;

namespace Popcorn.Exceptions
{
    [Serializable]
    public class NoDataInDroppedFileException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public NoDataInDroppedFileException()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        public NoDataInDroppedFileException(string message) : base(message)
        {
        }
    }
}
