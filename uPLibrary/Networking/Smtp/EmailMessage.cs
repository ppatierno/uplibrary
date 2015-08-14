using System;
using Microsoft.SPOT;
using System.Collections;

namespace uPLibrary.Networking.Smtp
{
    /// <summary>
    /// Email message
    /// </summary>
    public class EmailMessage
    {
        // end of each command message
        public const string CRLF = "\r\n";

        /// <summary>
        /// From address
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Address collection of recipients
        /// </summary>
        public ArrayList To { get; set; }

        /// <summary>
        /// Subject
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Body
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Email body has Html format
        /// </summary>
        public bool IsBodyHtml { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public EmailMessage()
        {
            this.From = String.Empty;
            this.To = new ArrayList();
            this.Subject = String.Empty;
            this.Body = String.Empty;
            this.IsBodyHtml = false;
        }

        public override string ToString()
        {
            // fill email headers
            string email = "Subject: " + this.Subject + CRLF;
            email += "From: " + "<" + this.From + ">" + CRLF;

            email += "To: ";
            foreach (string rcpt in this.To)
            {
                email += "<" + rcpt + ">,";
            }
            email = email.TrimEnd(',');
            email += CRLF;
            email += "Date: " + DateTime.Now.ToString("ddd, d MMM yyyy HH:mm:ss") + CRLF;

            if (!this.IsBodyHtml)
                email += "Content-Type: text/plain";
            else
                email += "Content-Type: text/html";

            email += "; charset=\"UTF-8\"" + CRLF;

            // extra CRLF to separate body from headers
            email += CRLF;
            // fill email body
            email += this.Body + CRLF;
            // email message ends with "CRLF.CRLF"
            email += "." + CRLF;

            return email;
        }
    }
}
