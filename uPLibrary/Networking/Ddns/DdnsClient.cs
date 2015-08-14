using System;
using Microsoft.SPOT;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Text;

namespace uPLibrary.Networking.Ddns
{
    /// <summary>
    /// EventArgs class for Ip address updated event
    /// </summary>
    public class IpAddressUpdatedEventArgs : EventArgs
    {
        /// <summary>
        /// IP Address (null if update failed)
        /// </summary>
        public IPAddress IpAddress { get; internal set; }

        /// <summary>
        /// Response from Dynamic Dns service provider
        /// </summary>
        public DdnsResponse Response { get; internal set; }
    }

    public delegate void IpAddressUpdatedHandler(object sender, IpAddressUpdatedEventArgs e);

    /// <summary>
    /// Abstract base class for all Dynamic Dnd Clients
    /// </summary>
    public abstract class DdnsClient : IDdnsClient
    {
        // max dimension for receive buffer
        protected const int RECEIVE_BUFFER_SIZE = 1024;
        // path to getting for update IP address
        protected const string DDNS_UPDATE_IP_PATH = "/nic/update";
        protected const string DDNS_CHECK_IP_PATH = "/";
        protected const string DDNS_CLIENT_USER_AGENT = "uDdns Client/1.0";

        // IP address updated event
        public event IpAddressUpdatedHandler IpAddressUpdated;

        // Ddns configuration information
        protected DdnsConfig ddnsConfig;

        // timer for periodic check/update IP address
        private Timer updaterIpTimer;

        // socket for updating IP address
        protected Socket updateIpSocket;
        // socket for checking IP address
        protected Socket checkIpSocket;

        // buffers for send/receive data by socket
        protected byte[] sendBuffer;
        protected byte[] receiveBuffer;

        // current IP address
        protected IPAddress ipAddress;

        // Ddns service provider endpoint for updating IP address
        protected IPEndPoint updateIpEndPoint;
        // Ddns service provider endpoint for checking IP address
        protected IPEndPoint checkIpEndPoint;

        // commands for checking and updating Ip address
        protected DdnsCheckIpCommand checkIpCmd;
        protected DdnsUpdateIpCommand updateIpCmd;

        // response from Dynamic Dns service provider
        private DdnsResponse response;

        // names of check IP address ed update IP address hosts
        protected string checkIpHost;
        protected string updateIpHost;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ddnsConfig">Ddns configuration information</param>
        public DdnsClient(DdnsConfig ddnsConfig)
        {
            this.ddnsConfig = ddnsConfig;

            // create timer for periodic and automatic check/update but not start it
            this.updaterIpTimer = new Timer(this.CheckUpdateIpAddressCallback, null, Timeout.Infinite, this.ddnsConfig.Period);

            this.receiveBuffer = new byte[RECEIVE_BUFFER_SIZE];
            this.checkIpHost = String.Empty;
            this.updateIpHost = String.Empty;
        }

        /// <summary>
        /// Timer callback for check/update IP address
        /// </summary>
        /// <param name="state"></param>
        protected void CheckUpdateIpAddressCallback(object state)
        {
            try
            {
                // check IP address
                IPAddress ipAddress = this.CheckIpAddress();
                // if IP address is null (no check executed) or 
                // the last IP address is null (first updating) or
                // last IP address is different from checked IP address
                if ((ipAddress == null) || (this.ipAddress == null) || (!this.ipAddress.Equals(ipAddress)))
                {
                    // if check IP address has returned a valid IP address
                    if (ipAddress != null)
                        this.updateIpCmd.IpAddress = ipAddress.ToString();
                    // update IP address
                    this.UpdateIpAddress();
                }
            }
            catch { }
        }

        /// <summary>
        /// Check current client IP address
        /// </summary>
        /// <returns>Current client IP address</returns>
        protected virtual IPAddress CheckIpAddress()
        {
            IPAddress ipAddress = null;
            // if check IP address end point is set
            if (this.checkIpEndPoint != null)
            {
                // create socket for checkin IP address
                using (this.checkIpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    this.checkIpSocket.Connect(this.checkIpEndPoint);

                    // send IP address check request
                    this.sendBuffer = Encoding.UTF8.GetBytes(this.checkIpCmd.ToString());
                    this.checkIpSocket.Send(this.sendBuffer);

                    this.receiveBuffer = new byte[RECEIVE_BUFFER_SIZE];
                    // read and decode response
                    if (this.checkIpSocket.Receive(this.receiveBuffer) > 0)
                    {
                        ipAddress = this.DecodeCheckIpResponse(new String(Encoding.UTF8.GetChars(this.receiveBuffer)));
                    }
                }
            }
            return ipAddress;
        }

        /// <summary>
        /// Update client Ip address
        /// </summary>
        protected virtual void UpdateIpAddress()
        {
            // create socket for updating IP address
            using (this.updateIpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                this.updateIpSocket.Connect(this.updateIpEndPoint);

                // prepare buffers for send and receive
                this.sendBuffer = Encoding.UTF8.GetBytes(this.updateIpCmd.ToString());
                this.receiveBuffer = new byte[RECEIVE_BUFFER_SIZE];

                int byteRead = 0;

                // SSL configured
                if (this.ddnsConfig.SSL)
                {
                    // create SSL stream
                    using (Microsoft.SPOT.Net.Security.SslStream sslStream = new Microsoft.SPOT.Net.Security.SslStream(this.updateIpSocket))
                    {
                        // SSL handshake authentication
                        sslStream.AuthenticateAsClient(this.updateIpHost, Microsoft.SPOT.Net.Security.SslProtocols.TLSv1);
                        // send message
                        sslStream.Write(this.sendBuffer, 0, this.sendBuffer.Length);

                        // cycle for reading from socket
                        int offset = 0;
                        int read = 0;
                        do
                        {
                            read = sslStream.Read(this.receiveBuffer, offset, RECEIVE_BUFFER_SIZE - offset);
                            offset += read;
                        } while (read != 0);

                        byteRead = offset;
                    }
                }
                else
                {
                    // send message and read response
                    this.updateIpSocket.Send(this.sendBuffer);
                    byteRead = this.updateIpSocket.Receive(this.receiveBuffer);
                }


                if (byteRead > 0)
                {
                    this.response = this.DecodeUpdateIpResponse(new String(Encoding.UTF8.GetChars(this.receiveBuffer)));
                    // raise IP address updated event
                    this.OnIpAddressUpdated(new IpAddressUpdatedEventArgs { IpAddress = this.ipAddress, Response = this.response });
                }
            }
        }

        /// <summary>
        /// Decode update IP address response
        /// </summary>
        /// <param name="updateIpResp">Update IP address response</param>
        /// <returns>Response code from Dynamic Dns service provider</returns>
        protected abstract DdnsResponse DecodeUpdateIpResponse(string updateIpResp);

        /// <summary>
        /// Decode check IP address response
        /// </summary>
        /// <param name="checkedIpResp">Check IP address response</param>
        /// <returns>IP address extracted from Dynamic Dns service provider response</returns>
        protected abstract IPAddress DecodeCheckIpResponse(string checkedIpResp);

        /// <summary>
        /// Raise IP address updated event
        /// </summary>
        /// <param name="e">Event Args object</param>
        private void OnIpAddressUpdated(IpAddressUpdatedEventArgs e)
        {
            if (this.IpAddressUpdated != null)
                this.IpAddressUpdated(this, e);
        }

        #region IDdnsClient...

        public IPAddress IpAddress
        {
            get 
            {
                return this.ipAddress;
            }
        }

        public void Start()
        {
            // start updater IP address timer
            this.updaterIpTimer.Change(0, this.ddnsConfig.Period);
        }

        public void Stop()
        {
            // stop updater IP address timer
            this.updaterIpTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        #endregion
    }
}
