using System;
using Microsoft.SPOT;

namespace uPLibrary.Networking.Smtp
{
    /// <summary>
    /// Represents an Smtp command
    /// </summary>
    public class SmtpCommand
    {
        // end of each command message
        public const string CRLF = "\r\n";

        /// <summary>
        /// Command code for Smtp protocol
        /// </summary>
        public string CommandCode { get; internal set; }

        /// <summary>
        /// Command text
        /// </summary>
        public string Text { get; internal set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="commandCode">Command code for Smtp protocol</param>
        /// <param name="text">Command text</param>
        public SmtpCommand(string commandCode, string text)
        {
            this.CommandCode = commandCode;
            this.Text = text;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SmtpCommand()
            : this(String.Empty, String.Empty)
        {
        }
        
        public override string ToString()
        {
            string command = String.Empty;

            // if is specified a command code
            if (this.CommandCode != String.Empty)
                command += this.CommandCode + " ";
            
            // if is specified a text
            if (this.Text != String.Empty)
                command += this.Text;

            command.TrimEnd();
            command += CRLF;
            
            return command;
        }
    }

    public class SmtpCommandCode
    {
        /// <summary>
        /// HELO command
        /// </summary>
        public const string HELO = "HELO";

        /// <summary>
        /// MAIL command
        /// </summary>
        public const string MAIL = "MAIL";

        /// <summary>
        /// RCPT command
        /// </summary>
        public const string RCPT = "RCPT";

        /// <summary>
        /// DATA command
        /// </summary>
        public const string DATA = "DATA";

        /// <summary>
        /// SEND command
        /// </summary>
        public const string SEND = "SEND";

        /// <summary>
        /// SOML command
        /// </summary>
        public const string SOML = "SOML";

        /// <summary>
        /// SAML command
        /// </summary>
        public const string SAML = "SAML";

        /// <summary>
        /// RSET command
        /// </summary>
        public const string RSET = "RSET";

        /// <summary>
        /// VRFY command
        /// </summary>
        public const string VRFY = "VRFY";

        /// <summary>
        /// EXPN command
        /// </summary>
        public const string EXPN = "EXPN";

        /// <summary>
        /// HELP command
        /// </summary>
        public const string HELP = "HELP";

        /// <summary>
        /// NOOP command
        /// </summary>
        public const string NOOP = "NOOP";

        /// <summary>
        /// QUIT command
        /// </summary>
        public const string QUIT = "QUIT";

        /// <summary>
        /// TURN command
        /// </summary>
        public const string TURN = "TURN";
    }
}
