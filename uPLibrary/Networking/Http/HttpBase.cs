using System;

namespace uPLibrary.Networking.Http
{
    /// <summary>
    /// Base class for HTTP object (request/response)
    /// </summary>
    public class HttpBase
    {
        #region Constants...

        // default HTTP protocol
        protected const string HTTP_1_1 = "HTTP/1.1";

        // const string separator
        internal const char CR = '\r';
        internal const char LF = '\n';
        
        // request and response line separator
        internal const char REQUEST_LINE_SEPARATOR = ' ';
        internal const char RESPONSE_LINE_SEPARATOR = ' ';
        // header-value separator 
        internal const char HEADER_VALUE_SEPARATOR = ':';
        // line separator
        internal const string CRLF = "\r\n";

        // content length header string
        internal const string CONTENT_LENGTH = "Content-Length";
        // content type header string
        internal const string CONTENT_TYPE = "Content-Type";
        // user agent header string
        internal const string USER_AGENT = "User-Agent";
        // host header string
        internal const string HOST = "Host";
        // transfer encoding header string
        internal const string TRANSFER_ENCODING = "Transfer-Encoding";
        internal const string TRANSFER_ENCODING_CHUNKED = "chunked";
        
        #endregion

        #region Properties...

        /// <summary>
        /// Headers of the HTTP request/response
        /// </summary>
        public HeadersCollection Headers { get; protected set; }

        /// <summary>
        /// HTTP protocol version
        /// </summary>
        public String HttpProtocol { get; internal set; }

        /// <summary>
        /// HTTP body request/response
        /// </summary>
        public IHttpBody Body { get; internal set; }

        /// <summary>
        /// Content-Length header
        /// </summary>
        public int ContentLength
        {
            get
            {
                if (this.Headers.Contains(CONTENT_LENGTH))
                    return int.Parse(this.Headers[CONTENT_LENGTH]);
                else
                    return 0;
            }
            set
            {
                if (this.Headers.Contains(CONTENT_LENGTH))
                    this.Headers[CONTENT_LENGTH] = value.ToString();
                else
                    this.Headers.Add(CONTENT_LENGTH, value.ToString());
            }
        }

        /// <summary>
        /// Transfer encoding
        /// </summary>
        public string TransferEncoding
        {
            get
            {
                if (this.Headers.Contains(TRANSFER_ENCODING))
                    return this.Headers[TRANSFER_ENCODING];
                else
                    return null;
            }
            set
            {
                if (this.Headers.Contains(TRANSFER_ENCODING))
                    this.Headers[TRANSFER_ENCODING] = value;
                else
                    this.Headers.Add(TRANSFER_ENCODING, value);
            }
        }

        /// <summary>
        /// User agent
        /// </summary>
        public string UserAgent
        {
            get
            {
                if (this.Headers.Contains(USER_AGENT))
                    return this.Headers[USER_AGENT];
                else
                    return null;
            }
            set
            {
                if (this.Headers.Contains(USER_AGENT))
                    this.Headers[USER_AGENT] = value;
                else
                    this.Headers.Add(USER_AGENT, value);
            }
        }

        /// <summary>
        /// Host header
        /// </summary>
        public string Host
        {
            get
            {
                if (this.Headers.Contains(HOST))
                    return this.Headers[HOST];
                else
                    return null;
            }
            set
            {
                if (this.Headers.Contains(HOST))
                    this.Headers[HOST] = value;
                else
                    this.Headers.Add(HOST, value);
            }
        }

        /// <summary>
        /// Content type header
        /// </summary>
        public string ContentType
        {
            get
            {
                if (this.Headers.Contains(CONTENT_TYPE))
                    return this.Headers[CONTENT_TYPE];
                else
                    return null;
            }
            set
            {
                if (this.Headers.Contains(CONTENT_TYPE))
                    this.Headers[CONTENT_TYPE] = value;
                else
                    this.Headers.Add(CONTENT_TYPE, value);
            }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public HttpBase()
        {
            this.Headers = new HeadersCollection();
            this.HttpProtocol = HTTP_1_1;
        }
    }
}
