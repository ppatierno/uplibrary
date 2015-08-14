using System;
using Microsoft.SPOT;

namespace uPLibrary.Networking.Smtp
{
    /// <summary>
    /// Exception thrown due to error on Smtp communication
    /// </summary>
    public class SmtpCommunicationException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SmtpCommunicationException()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="innerException">Inner exception</param>
        public SmtpCommunicationException(Exception innerException)
            : base(String.Empty, innerException)
        {
        }
    }
}
