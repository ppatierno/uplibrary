using System;
using Microsoft.SPOT;

namespace uPLibrary.Networking.Smtp
{
    /// <summary>
    /// Information about Smtp server to connect for sending emails
    /// </summary>
    public class SmtpServer
    {
        // default Smtp server port
        public const int SMTP_DEFAULT_PORT = 25;

        // server host name and port
        private string host;
        private int port;

        /// <summary>
        /// Server host name
        /// </summary>
        public string Host 
        {
            get { return this.host; }
            set { this.host = value; }
        }

        /// <summary>
        /// Server port (default 25)
        /// </summary>
        public int Port
        {
            get { return this.port; }
            set { this.port = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="host">Server host name</param>
        /// <param name="port">Server port name</param>
        public SmtpServer(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        /// <summary>
        /// Costructor with Smtp port 25 as default
        /// </summary>
        /// <param name="host">Server host name</param>
        public SmtpServer(string host)
            : this(host, SMTP_DEFAULT_PORT)
        {
        }
        
    }
}
