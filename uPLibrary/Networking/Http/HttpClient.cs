using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace uPLibrary.Networking.Http
{
    /// <summary>
    /// Delegate for sending request body
    /// </summary>
    /// <param name="httpReq">HTTP request object</param>
    /// <returns></returns>
    public delegate void SendBodyEventHandler(HttpRequest httpReq);
    public delegate void RecvBodyEventHandler(HttpResponse httpResp);

    /// <summary>
    /// HTTP client
    /// </summary>
    public class HttpClient
    {
        // HTTP buffer size
        private const int BUFFER_SIZE = 1024;
        // user agent
        private const string HTTP_CLIENT_NAME = "uPHttpClient";
        // socket poll timeout
        private const int POLL_TIMEOUT = 30 * 1000000; // 30 sec (expressed in us)

        // socket for connection to the host
        private Socket socket;
        // host endpoint
        private IPEndPoint hostIpEndPoint;
        // socket buffer
        private byte[] buffer;

        // event raised to client for sending and receiving body
        public event SendBodyEventHandler SendBody;
        public event RecvBodyEventHandler RecvBody;

        /// <summary>
        /// Constructor
        /// </summary>
        public HttpClient()
        {
            this.buffer = new byte[BUFFER_SIZE];
        }

        /// <summary>
        /// Send a HTTP request
        /// </summary>
        /// <param name="httpReq">HTTP request</param>
        /// <returns>HTTP response</returns>
        public HttpResponse Send(HttpRequest httpReq)
        {
            // check on parameter
            if (httpReq == null)
                throw new ArgumentNullException("HttpRequest cannot be null !");

            // register write callaback for body request
            httpReq.Body.Write = this.WriteBody;

            // if Host is IP address
            if (httpReq.Uri.HostNameType == UriHostNameType.IPv4)
            {
                this.hostIpEndPoint = new IPEndPoint(IPAddress.Parse(httpReq.Uri.Host), httpReq.Uri.Port);
            }
            // Host is DNS address
            else
            {
                // resolve Host name by DNS
                IPHostEntry hostEntry = Dns.GetHostEntry(httpReq.Uri.Host);
                // check for the first address not null
                // it seems that with .Net Micro Framework, the IPV6 addresses aren't supported and return "null"
                int i = 0;
                while (hostEntry.AddressList[i] == null) i++;
                this.hostIpEndPoint = new IPEndPoint(hostEntry.AddressList[i], httpReq.Uri.Port);
            }

            // add "Host" header
            httpReq.Host = httpReq.Uri.Host + ":" + httpReq.Uri.Port;
            // add "User-Agent" header
            httpReq.UserAgent = HTTP_CLIENT_NAME;

            // create socket and connect
            this.socket = new Socket(this.hostIpEndPoint.Address.GetAddressFamily(), SocketType.Stream, ProtocolType.Tcp);
            this.socket.Connect(this.hostIpEndPoint);

            // send HTTP request (request line and headers)
            this.buffer = Encoding.UTF8.GetBytes(httpReq.ToString());
            this.socket.Send(this.buffer);

            // raise event for allow client to send body request
            this.OnSendBody(httpReq);
            
            int received = 0;
            this.buffer = new byte[BUFFER_SIZE];
            HttpResponseParserResult result = HttpResponseParserResult.NotCompleted;
            HttpResponseParser parser = new HttpResponseParser();
            parser.ReceivingBody += parser_ReceivingBody;
            
            // receive on socket until parse is complete or no data received
            while ((result != HttpResponseParserResult.Completed) &&
                    this.socket.Poll(POLL_TIMEOUT, SelectMode.SelectRead))
            {
                // no data on the socket (closed or timeout)
                if (this.socket.Available == 0)
                    break;

                received = this.socket.Receive(this.buffer, BUFFER_SIZE, SocketFlags.None);
                result = parser.Parse(this.buffer, received);

                // if the HTTP response is malformed, break
                if (result == HttpResponseParserResult.Malformed)
                    break;
            }

            // unregister write callaback for body request
            httpReq.Body.Write = null;

            // close connection
            this.socket.Close();

            return parser.Response;
        }

        /// <summary>
        /// Execute a GET HTTP request
        /// </summary>
        /// <param name="uri">URI for request</param>
        /// <param name="recvBody">Event handler for receiving body</param>
        /// <returns>HTTP response</returns>
        public HttpResponse Get(string uri, RecvBodyEventHandler recvBody)
        {
            return this.Get(new Uri(uri), recvBody);
        }

        /// <summary>
        /// Execute a GET HTTP request
        /// </summary>
        /// <param name="uri">URI for request</param>
        /// <param name="recvBody">Event handler for receiving body</param>
        /// <returns>HTTP response</returns>
        public HttpResponse Get(Uri uri, RecvBodyEventHandler recvBody)
        {
            if (recvBody == null)
                throw new ArgumentNullException("recvBody parameter cannot be null");

            HttpRequest httpReq = new HttpRequest();
            httpReq.Method = HttpMethod.Get;
            httpReq.Uri = uri;
            this.RecvBody += recvBody;
            return this.Send(httpReq);
        }

        /// <summary>
        /// Execute a POST HTTP request
        /// </summary>
        /// <param name="uri">URI for request</param>
        /// <param name="sendBody">Event handler for sending body</param>
        /// <returns>HTTP response</returns>
        public HttpResponse Post(string uri, SendBodyEventHandler sendBody)
        {
            return this.Post(new Uri(uri), sendBody);
        }

        /// <summary>
        /// Execute a POST HTTP request
        /// </summary>
        /// <param name="uri">URI for request</param>
        /// <param name="sendBody">Event handler for sending body</param>
        /// <returns>HTTP response</returns>
        public HttpResponse Post(Uri uri, SendBodyEventHandler sendBody)
        {
            if (sendBody == null)
                throw new ArgumentNullException("sendBody parameter cannot be null");

            HttpRequest httpReq = new HttpRequest();
            httpReq.Method = HttpMethod.Post;
            httpReq.Uri = uri;
            this.SendBody += sendBody;
            return this.Send(httpReq);
        }

        void parser_ReceivingBody(HttpResponse httpResp)
        {
            // receiving body response from parse, raise event to the client
            this.OnRecvBody(httpResp);
        }

        /// <summary>
        /// Callback called for writing body request
        /// </summary>
        /// <param name="buffer">Bytes to write</param>
        /// <param name="offset">Offset from start to write</param>
        /// <param name="count">Number of bytes to write</param>
        private void WriteBody(byte[] buffer, int offset, int count)
        {
            this.socket.Send(buffer, offset, count, SocketFlags.None);
        }

        /// <summary>
        /// Raise receive body event
        /// </summary>
        /// <param name="httpReq">HTTP response for reading body</param>
        private void OnRecvBody(HttpResponse httpResp)
        {
            if (this.RecvBody != null)
                this.RecvBody(httpResp);
        }       

        /// <summary>
        /// Raise send body event
        /// </summary>
        /// <param name="httpReq">HTTP request for writing body</param>
        private void OnSendBody(HttpRequest httpReq)
        {
            if (this.SendBody != null)
                this.SendBody(httpReq);
        }
    }

    /// <summary>
    /// IPAddress Utility class
    /// </summary>
    public static class IPAddressUtility
    {
        /// <summary>
        /// Return AddressFamily for the IP address
        /// </summary>
        /// <param name="ipAddress">IP address to check</param>
        /// <returns>Address family</returns>
        public static AddressFamily GetAddressFamily(this IPAddress ipAddress)
        {
            return (ipAddress.ToString().IndexOf(':') != -1) ?
                AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;
        }
    }
}
