using System;
using Microsoft.SPOT;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections;

namespace uPLibrary.IoT.ThingSpeak
{
    /// <summary>
    /// Class of the ThingSpeak platform client
    /// </summary>
    public class ThingSpeakClient
    {
        internal const string CRLF = "\r\n";

        // max dimension for receive buffer
        private const int RECEIVE_BUFFER_SIZE = 1024;

        // host name and ports
        internal const string THING_SPEAK_HOST = "api.thingspeak.com";
        internal const int THING_SPEAK_PORT = 80;
        internal const int THING_SPEAK_SSL_PORT = 443;

        // path for channel updating
        internal const string THING_SPEAK_UPDATE_PATH = "/update";
        
        // max dimensions
        internal const int THING_SPEAK_MAX_FIELDS = 8;
        internal const int THING_SPEAK_MAX_STATUS = 140;

        // socket for connecting to the host
        private Socket socket;
        // host endpoint
        private IPEndPoint hostIpEndPoint;

        // if HTTPS connection is requested
        private bool SSL;

        // buffers for send/receive data by socket
        private byte[] sendBuffer;
        private byte[] receiveBuffer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="SSL">HTTPS connection requested</param>
        public ThingSpeakClient(bool SSL)
        {
            this.SSL = SSL;
            // get ip address for the host
            IPHostEntry hostEntry = Dns.GetHostEntry(THING_SPEAK_HOST);
            // create host endpoint
            this.hostIpEndPoint = new IPEndPoint(hostEntry.AddressList[0], this.SSL ? THING_SPEAK_SSL_PORT : THING_SPEAK_PORT);
        }

        /// <summary>
        /// Update a channel
        /// </summary>
        /// <param name="writeApiKey">Write API Key for the channel to update</param>
        /// <param name="dataEntry">Data entry for updating channel</param>
        /// <returns>Update result</returns>
        public bool Update(
            string writeApiKey,
            DataEntry dataEntry)
        {
            // check for a mandatory write API Key
            if ((writeApiKey == null) || (writeApiKey == String.Empty))
                throw new ArgumentNullException("writeApiKey", "You must specify a write API Key");

            // check max fields number
            if (dataEntry.Fields.Length > THING_SPEAK_MAX_FIELDS)
                throw new ArgumentException("fields", "Max number of field is " + THING_SPEAK_MAX_FIELDS);

            // check at leaset one field value not empty
            bool checkFields = false;
            for (int i = 0; i < dataEntry.Fields.Length; i++)
            {
                if (dataEntry.Fields[i] != null)
                {
                    checkFields = true;
                    break;
                }
            }
            if (!checkFields)
                throw new ArgumentNullException("fields", "You must specify a field value at least");

            // check status message
            if ((dataEntry.Status != null) && (dataEntry.Status.Length > THING_SPEAK_MAX_STATUS))
                throw new ArgumentException("status", "Max status length is " + THING_SPEAK_MAX_STATUS);

            // check twitter account and message
            if (((dataEntry.Twitter == null) && (dataEntry.Tweet != null)) || ((dataEntry.Twitter != null) && (dataEntry.Tweet == null)))
                throw new ArgumentException("twitter and tweet parameters must be both valued");
            
            // build body
            string body = String.Empty;
            // fields...
            for (int i = 0; i < dataEntry.Fields.Length; i++)
            {
                if ((dataEntry.Fields[i] != null) && (dataEntry.Fields[i] != String.Empty))
                {
                    if (i > 0)
                        body += "&";
                    body += "field" + (i + 1) + "=" + dataEntry.Fields[i];
                }
            }
            // ...location...
            if (dataEntry.Location != null)
                body += "&lat=" + dataEntry.Location.Latitude + "&long=" + dataEntry.Location.Longitude + "&elevation=" + dataEntry.Location.Elevation;
            // ...status...
            if (dataEntry.Status != null)
                body += "&status=" + dataEntry.Status;
            // ...twitter...
            if ((dataEntry.Twitter != null) && (dataEntry.Tweet != null))
                body += "&twitter=" + dataEntry.Twitter + "&tweet=" + dataEntry.Tweet;

            // build HTTP request
            string request = "POST " + THING_SPEAK_UPDATE_PATH + " HTTP/1.1" + CRLF;
            request += "Host: " + THING_SPEAK_HOST + CRLF;
            request += "Connection: close" + CRLF;
            request += "X-THINGSPEAKAPIKEY: " + writeApiKey + CRLF;
            request += "Content-Type: application/x-www-form-urlencoded" + CRLF;
            request += "Content-Length: " + body.Length + CRLF;
            request += CRLF;
            request += body + CRLF;

            string result = String.Empty;

            // open socket e connect to the host
            using (this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                this.socket.Connect(this.hostIpEndPoint);

                // send HTTP request
                this.sendBuffer = Encoding.UTF8.GetBytes(request);

                if (this.SSL)
                {
                    // TODO : HTTPS communication
                }
                else
                {
                    // send HTTP request
                    this.socket.Send(this.sendBuffer);

                    // receive HTTP response
                    this.receiveBuffer = new byte[RECEIVE_BUFFER_SIZE];

                    // poll on socket for reading (timeout 30 sec)
                    while (this.socket.Poll(30 * 1000000, SelectMode.SelectRead))
                    {
                        // no data on th socket (closed or timeout)
                        if (this.socket.Available == 0) break;

                        // empty buffer
                        Array.Clear(this.receiveBuffer, 0, this.receiveBuffer.Length);

                        // read data
                        this.socket.Receive(this.receiveBuffer);

                        // append response
                        result += new String(Encoding.UTF8.GetChars(this.receiveBuffer));
                    }
                }
            }

            // decode HTTP response
            ThingSpeakHttpResponse httpResponse = ThingSpeakHttpResponse.Parse(result);
            Debug.Print(result);

            if (httpResponse.StatusCode == 200)
            {
                // set entry Id received from the server
                dataEntry.Id = Convert.ToInt32(httpResponse.Body);
                return true;
            }
            else
                return false;           
        }

        /// <summary>
        /// Read last entry in a field feed
        /// </summary>
        /// <param name="readApiKey">Read API Key for the channel to read (null if channel is public)</param>
        /// <param name="channelId">Channel ID</param>
        /// <param name="fieldId">Field ID</param>
        /// <param name="status">Include status update in feed</param>
        /// <param name="location">Include latitude, longitude and elevation in feed</param>
        /// <returns>List of all data entries read</returns>
        public ArrayList ReadLastFieldEntry(string readApiKey, int channelId, int fieldId, bool status = false, bool location = false)
        {
            return this.ReadChannel(readApiKey, channelId, "/field/" + fieldId + "/last.csv" + this.ReadMakeQueryString(status, location));
        }

        /// <summary>
        /// Read a field feed
        /// </summary>
        /// <param name="readApiKey">Read API Key for the channel to read (null if channel is public)</param>
        /// <param name="channelId">Channel ID</param>
        /// <param name="fieldId">Field ID</param>
        /// <param name="status">Include status update in feed</param>
        /// <param name="location">Include latitude, longitude and elevation in feed</param>
        /// <returns>List of all data entries read</returns>
        public ArrayList ReadField(string readApiKey, int channelId, int fieldId, bool status = false, bool location = false)
        {
            return this.ReadChannel(readApiKey, channelId, "/field/" + fieldId + ".csv" + this.ReadMakeQueryString(status, location));
        }

        /// <summary>
        /// Read channel feeds
        /// </summary>
        /// <param name="readApiKey">Read API Key for the channel to read (null if channel is public)</param>
        /// <param name="channelId">Channel ID</param>
        /// <param name="status">Include status update in feed</param>
        /// <param name="location">Include latitude, longitude and elevation in feed</param>
        /// <returns>List of all data entries read</returns>
        public ArrayList ReadFeeds(string readApiKey, int channelId, bool status = false, bool location = false)
        {
            return this.ReadChannel(readApiKey, channelId, "/feed.csv" + this.ReadMakeQueryString(status, location));
        }

        /// <summary>
        /// Read last entry in channel feed
        /// </summary>
        /// <param name="readApiKey">Read API Key for the channel to read (null if channel is public)</param>
        /// <param name="channelId">Channel ID</param>
        /// <param name="status">Include status update in feed</param>
        /// <param name="location">Include latitude, longitude and elevation in feed</param>
        /// <returns>List of all data entries read</returns>
        public ArrayList ReadLastFeedEntry(string readApiKey, int channelId, bool status = false, bool location = false)
        {
            return this.ReadChannel(readApiKey, channelId, "/feed/last.csv" + this.ReadMakeQueryString(status, location));
        }

        /// <summary>
        /// Read status updates
        /// </summary>
        /// <param name="readApiKey">Read API Key for the channel to read (null if channel is public)</param>
        /// <param name="channelId">Channel ID</param>
        /// <returns>List of all data entries read with only status update</returns>
        public ArrayList ReadStatusUpdate(string readApiKey, int channelId)
        {
            return this.ReadChannel(readApiKey, channelId, "/status.csv");
        }

        /// <summary>
        /// Make query string for reading commands
        /// </summary>
        /// <param name="status">Include status update in feed</param>
        /// <param name="location">Include latitude, longitude and elevation in feed</param>
        /// <returns>Query string made</returns>
        private string ReadMakeQueryString(bool status, bool location)
        {
            string queryString = String.Empty;

            if (status)
                queryString += "?status=true";
            
            if (location)
            {
                if (queryString != String.Empty)
                    queryString += "&";
                else
                    queryString += "?";

                queryString += "location=true";
            }
            return queryString;
        }

        /// <summary>
        /// Read channel data entries
        /// </summary>
        /// <param name="readApiKey">Read API Key for the channel to read (null if channel is public)</param>
        /// <param name="channelId">Channel ID</param>
        /// <param name="path">Path for reading channel</param>
        /// <returns>List of all data entries read</returns>
        private ArrayList ReadChannel(string readApiKey, int channelId, string path)
        {
            // build HTTP request
            string request = "GET /channels/" + channelId + path + " HTTP/1.1" + CRLF;
            request += "Host: " + THING_SPEAK_HOST + CRLF;
            request += "Connection: close" + CRLF;
            if ((readApiKey != null) && (readApiKey != String.Empty))
                request += "X-THINGSPEAKAPIKEY: " + readApiKey + CRLF;
            request += CRLF;

            string result = String.Empty;

            // open socket e connect to the host
            using (this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                this.socket.Connect(this.hostIpEndPoint);
                                
                this.sendBuffer = Encoding.UTF8.GetBytes(request);
                
                if (this.SSL)
                {
                    // TODO : HTTPS communication
                }
                else
                {
                    // send HTTP request
                    this.socket.Send(this.sendBuffer);

                    // receive HTTP response
                    this.receiveBuffer = new byte[RECEIVE_BUFFER_SIZE];

                    // poll on socket for reading (timeout 30 sec)
                    while (this.socket.Poll(30 * 1000000, SelectMode.SelectRead))
                    {
                        // no data on th socket (closed or timeout)
                        if (this.socket.Available == 0) break;

                        // empty buffer
                        Array.Clear(this.receiveBuffer, 0, this.receiveBuffer.Length);

                        // read data
                        this.socket.Receive(this.receiveBuffer);

                        // append response
                        result += new String(Encoding.UTF8.GetChars(this.receiveBuffer));
                    }
                }
            }

            // decode HTTP response
            ThingSpeakHttpResponse httpResponse = ThingSpeakHttpResponse.Parse(result);
            Debug.Print(result);

            if (httpResponse.StatusCode == 200)
                return DataEntry.ParseCsv(httpResponse.Body);
            else
                return null;
        }

    }

    
}
