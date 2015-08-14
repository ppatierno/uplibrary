using System;
using Microsoft.SPOT;

namespace uPLibrary.Networking.Smtp
{
    /// <summary>
    /// Exception thrown due to response from Smtp server
    /// </summary>
    public class SmtpException : Exception
    {
        // reply error code
        private SmtpReplyCode replyCode;

        /// <summary>
        /// Reply error code
        /// </summary>
        public SmtpReplyCode ReplyCode
        {
            get { return this.replyCode; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SmtpException()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="replyCode">Reply error code</param>
        public SmtpException(SmtpReplyCode replyCode)
        {
            this.replyCode = replyCode;
        }
    }
}
