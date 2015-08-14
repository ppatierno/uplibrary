using System;
using Microsoft.SPOT;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace uPLibrary.Networking.Smtp
{
    /// <summary>
    /// Smtp client for sending emails
    /// </summary>
    public class SmtpClient : IDisposable
    {
        // timeout on reading response (ms)
        public const int TIMEOUT_READ = 15000;
        public const int MICROSECONDS_PER_MILLISECONDS = 1000;
        // default client host name
        public const string SMTP_DEFAULT_CLIENT_HOST_NAME = "localhost";

        private const char CR = '\r';
        private const char LF = '\n';

        // socket for connecting to Smtp server
        private Socket socket;
        // Smtp server used for sending emails
        private SmtpServer smtpServer;
        // client host name
        private string clientHostName;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="smtpServer">Smtp server used for sending emails</param>
        /// <param name="clientHostName">Client host name</param>
        public SmtpClient(SmtpServer smtpServer, string clientHostName)
        {
            this.smtpServer = smtpServer;
            this.clientHostName = clientHostName;
        }

        /// <summary>
        /// Constructor with "localhost" as default client host name
        /// </summary>
        /// <param name="smtpServer">Smtp server used for sending emails</param>
        public SmtpClient(SmtpServer smtpServer)
            : this(smtpServer, SMTP_DEFAULT_CLIENT_HOST_NAME)
        {
        }

        public void Dispose()
        {
            this.socket = null;
        }

        /// <summary>
        /// Send an email message
        /// </summary>
        /// <param name="emailMessage">Email message to send</param>
        public void Send(EmailMessage emailMessage)
        {
            try
            {
                // get Smtp server Ip address
                IPHostEntry smtpServerHostEntry = Dns.GetHostEntry(this.smtpServer.Host);
                // create socket to connect to the Smtp server
                this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.socket.Connect(new IPEndPoint(smtpServerHostEntry.AddressList[0], this.smtpServer.Port));
            }
            catch (Exception ex)
            {
                throw new SmtpCommunicationException(ex);
            }

            SmtpReply smtpResponse;
            SmtpCommand smtpCommand;

            smtpResponse = SmtpReply.Parse(this.GetResponse());
            if (smtpResponse.ReplyCode != SmtpReplyCode.DomainServiceReady)
                throw new SmtpException(smtpResponse.ReplyCode);

            // HELO command
            smtpCommand = new SmtpCommand();
            smtpCommand.CommandCode = SmtpCommandCode.HELO;
            smtpCommand.Text = this.clientHostName;
            this.SendCommand(smtpCommand);
            smtpResponse = SmtpReply.Parse(this.GetResponse());
            if (smtpResponse.ReplyCode != SmtpReplyCode.RequestedMailActionOk)
                throw new SmtpException(smtpResponse.ReplyCode);

            // FROM command
            smtpCommand.CommandCode = SmtpCommandCode.MAIL;
            smtpCommand.Text = "FROM:<" +  emailMessage.From + ">";
            this.SendCommand(smtpCommand);
            smtpResponse = SmtpReply.Parse(this.GetResponse());
            if (smtpResponse.ReplyCode != SmtpReplyCode.RequestedMailActionOk)
                throw new SmtpException(smtpResponse.ReplyCode);

            // TO command, for all recipients
            foreach (string rcpt in emailMessage.To)
            {
                smtpCommand.CommandCode = SmtpCommandCode.RCPT;
                smtpCommand.Text = "TO:<" + rcpt + ">";
                this.SendCommand(smtpCommand);
                smtpResponse = SmtpReply.Parse(this.GetResponse());
                if (smtpResponse.ReplyCode != SmtpReplyCode.RequestedMailActionOk)
                    throw new SmtpException(smtpResponse.ReplyCode);
            }           

            // DATA command
            smtpCommand.CommandCode = SmtpCommandCode.DATA;
            smtpCommand.Text = String.Empty;
            this.SendCommand(smtpCommand);
            smtpResponse = SmtpReply.Parse(this.GetResponse());
            if (smtpResponse.ReplyCode != SmtpReplyCode.StartMailInput)
                    throw new SmtpException(smtpResponse.ReplyCode);

            // mail text
            this.SendRawData(emailMessage.ToString());
            smtpResponse = SmtpReply.Parse(this.GetResponse());
            if (smtpResponse.ReplyCode != SmtpReplyCode.RequestedMailActionOk)
                throw new SmtpException(smtpResponse.ReplyCode);

            // QUIT command
            smtpCommand.CommandCode = SmtpCommandCode.QUIT;
            smtpCommand.Text = String.Empty;
            this.SendCommand(smtpCommand);
            smtpResponse = SmtpReply.Parse(this.GetResponse());
            if (smtpResponse.ReplyCode != SmtpReplyCode.DomainServiceClosingTransmissionChannel)
                throw new SmtpException(smtpResponse.ReplyCode);

            this.socket.Close();
        }

        /// <summary>
        /// Send a command to the Smtp server
        /// </summary>
        /// <param name="smtpCommand">Command to send</param>
        private void SendCommand(SmtpCommand smtpCommand)
        {
            this.SendRawData(smtpCommand.ToString());
        }

        /// <summary>
        /// Send raw data to the Smtp server
        /// </summary>
        /// <param name="data">Raw data to send</param>
        private void SendRawData(string data)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(data);
            this.socket.Send(buffer);
        }

        /// <summary>
        /// Get response from Smtp server
        /// </summary>
        /// <returns>Response from Smtp server</returns>
        private string GetResponse()
        {
            byte[] respBuffer = new byte[2048];
            int respBytesRead = 0;
            byte[] buffer = new byte[512];
            string response = String.Empty;

            int timeout = TIMEOUT_READ * MICROSECONDS_PER_MILLISECONDS;
            while (this.socket.Poll(timeout, SelectMode.SelectRead))
            {
                int bytesRead = this.socket.Receive(buffer);
                if (bytesRead == 0)
                    throw new SmtpCommunicationException();

                Array.Copy(buffer, 0, respBuffer, respBytesRead, buffer.Length);
                respBytesRead += bytesRead;

                if ((respBuffer[respBytesRead - 2] == CR) &&
                    (respBuffer[respBytesRead - 1] == LF))
                    break;
            }

            response = new String(Encoding.UTF8.GetChars(respBuffer));
            return response;
        }

        
    }
}
