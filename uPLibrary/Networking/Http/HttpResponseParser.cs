using System;
using System.Text;

namespace uPLibrary.Networking.Http
{
    /// <summary>
    /// Parser for HTTP response
    /// </summary>
    internal class HttpResponseParser
    {
        // parser state and line extract state
        private HttpResponseParserState state;
        private HttpResponseLineState lineState;

#if !MF_FRAMEWORK_VERSION_V4_1
        // HTTP line extracted
        private StringBuilder lineBuilder;
#else
        // .NET MF doesn't support StringBuilder
        private string lineBuilder;
#endif
        // HTTP response parsed
        private HttpResponse response;
        // callback to allow client to read body response
        
        // buffer to parse and relative size
        private byte[] buffer;
        private int size;
        // remaining bytes for response body (entire or chunk)
        private int bodyRemaining;
        // transfer encoding chunked
        private bool isChunked;
        // parsed and remaining bytes into the buffer
        private int parsed;
        private int remaining;
        // parser result
        HttpResponseParserResult result;

        internal event RecvBodyEventHandler ReceivingBody;

        /// <summary>
        /// HTTP response
        /// </summary>
        public HttpResponse Response
        {
            get { return this.response; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        internal HttpResponseParser()
        {
            // initialize parser state
            this.state = HttpResponseParserState.ResponseLine;
            this.lineState = HttpResponseLineState.Idle;
#if !MF_FRAMEWORK_VERSION_V4_1
            this.lineBuilder = new StringBuilder();
#else
            this.lineBuilder = String.Empty;
#endif

            // create HTTP response object and assign read method for body response
            this.response = new HttpResponse();
            this.response.Body.Read = this.ReadBody;
        }

        /// <summary>
        /// Extract a line (CRLF terminated) from HTTP response
        /// </summary>
        /// <param name="buffer">Buffer with HTTP response bytes</param>
        /// <param name="offset">Offset from extract</param>
        /// <param name="size">Number of bytes</param>
        /// <returns>Number of parsed bytes (it can be less then size if a line is recognized inside buffer</returns>
        private int ExtractLine(byte[] buffer, int offset, int size)
        {
            int i = offset;

            while ((i < size) && (this.lineState != HttpResponseLineState.End))
            {
                switch (this.lineState)
                {
                    case HttpResponseLineState.Idle:

                        // CR recognized
                        if (buffer[i] == HttpBase.CR)
                            this.lineState = HttpResponseLineState.CarriageReturn;
                        // some server don't respond with CRLF but only with LF
                        else if (buffer[i] == HttpBase.LF)
                            this.lineState = this.lineState = HttpResponseLineState.End;
                        else
#if !MF_FRAMEWORK_VERSION_V4_1
                            this.lineBuilder.Append((char)buffer[i]);
#else
                            this.lineBuilder += (char)buffer[i];
#endif
                        break;

                    case HttpResponseLineState.CarriageReturn:

                        // LF recognized
                        if (buffer[i] == HttpBase.LF)
                            this.lineState = HttpResponseLineState.End;
                        break;

                    case HttpResponseLineState.End:
                        break;
                    default:
                        break;
                }
                i++;
            }

            // return number of parsed bytes. it can be less then size if
            // a line is recognized inside buffer
            return (i - offset);
        }

        /// <summary>
        /// Execute HTTP response parsing
        /// </summary>
        /// <param name="buffer">Buffer with HTTP response bytes</param>
        /// <param name="size">Number of bytes</param>
        /// <returns>Parsing result</returns>
        internal HttpResponseParserResult Parse(byte[] buffer, int size)
        {
            this.result = HttpResponseParserResult.NotCompleted;
            this.parsed = 0;
            this.remaining = 0;
            this.buffer = buffer;
            this.size = size;

            // entire size is remaining bytes to parser
            this.remaining = this.size;

            while (this.remaining > 0)
            {
                switch (this.state)
                {
                    // waiting for response line, start of HTTP response
                    case HttpResponseParserState.ResponseLine:

                        this.parsed += this.ExtractLine(this.buffer, this.parsed, this.size);
                        // response line extracted
                        if (this.lineState == HttpResponseLineState.End)
                        {
                            try
                            {
                                string[] token = this.lineBuilder.ToString().Split(HttpBase.RESPONSE_LINE_SEPARATOR);

                                // set HTTP protocol, status code and reason phrase
                                this.response.HttpProtocol = token[0];
                                this.response.StatusCode = (HttpStatusCode)Convert.ToInt32(token[1]);
                                if (token.Length == 3)
                                    this.response.ReasonPhrase = token[2];
                                else
                                {
                                    this.response.ReasonPhrase = string.Empty;
                                    for (int i = 2; i < token.Length; i++)
                                        this.response.ReasonPhrase += token[i];
                                }

                                // change parser state for waiting headers
                                this.state = HttpResponseParserState.Headers;
                                // reset extract line parser state
                                this.lineState = HttpResponseLineState.Idle;
#if !MF_FRAMEWORK_VERSION_V4_1
                                this.lineBuilder.Clear();
#else
                                this.lineBuilder = String.Empty;
#endif
                            }
                            catch
                            {
                                this.result = HttpResponseParserResult.Malformed;
                            }
                        }
                        this.remaining = (this.size - this.parsed);
                        break;

                    // receiving HTTP headers
                    case HttpResponseParserState.Headers:

                        this.parsed += this.ExtractLine(this.buffer, this.parsed, this.size);
                        // line extracted
                        if (this.lineState == HttpResponseLineState.End)
                        {
                            // header line
                            if (this.lineBuilder.Length > 0)
                            {
                                string headerLine = this.lineBuilder.ToString();

                                // add header to response (key and value)
                                this.response.Headers.Add(
                                    headerLine.Substring(0, headerLine.IndexOf(HttpBase.HEADER_VALUE_SEPARATOR)).Trim(),
                                    headerLine.Substring(headerLine.IndexOf(HttpBase.HEADER_VALUE_SEPARATOR) + 1).Trim()
                                    );
                            }
                            // empty line, headers end
                            else
                            {
                                // body chunked or not
                                if ((this.response.ContentLength > 0) || (this.response.TransferEncoding != null))
                                {
                                    this.bodyRemaining = (this.response.ContentLength > 0) ? this.response.ContentLength : 0;
                                    
                                    if (this.response.TransferEncoding != null)
                                        this.isChunked = (this.response.TransferEncoding.Equals(HttpBase.TRANSFER_ENCODING_CHUNKED));

                                    this.state = HttpResponseParserState.Body;
                                }
                                // no body, HTTP response end
                                else
                                {
                                    this.result = HttpResponseParserResult.Completed;
                                    this.state = HttpResponseParserState.ResponseLine;
                                }
                            }

                            // reset extract line parser state
                            this.lineState = HttpResponseLineState.Idle;
#if !MF_FRAMEWORK_VERSION_V4_1
                            this.lineBuilder.Clear();
#else
                            this.lineBuilder = String.Empty;
#endif
                        }
                        this.remaining = (this.size - this.parsed);
                        break;

                    // receiving body
                    case HttpResponseParserState.Body:

                        // body chunked and no chunk size already found
                        if ((this.isChunked) && (this.bodyRemaining == 0))
                        {
                            
                            this.parsed += this.ExtractLine(this.buffer, this.parsed, this.size);
                            // line extracted
                            if (this.lineState == HttpResponseLineState.End)
                            {
                                // line contains chunk size
                                if (this.lineBuilder.Length > 0)
                                {
                                    this.bodyRemaining = Convert.ToInt32(this.lineBuilder.ToString(), 16);
                                }
                                // empty line, body chunked end
                                else
                                {
                                    this.result = HttpResponseParserResult.Completed;
                                    this.state = HttpResponseParserState.ResponseLine;
                                }

                                // reset extract line parser state
                                this.lineState = HttpResponseLineState.Idle;
#if !MF_FRAMEWORK_VERSION_V4_1
                                this.lineBuilder.Clear();
#else
                                this.lineBuilder = String.Empty;
#endif
                            }

                            this.remaining = (this.size - this.parsed);
                        }
                        // read body or chunk
                        else
                        {
                            // raise event for allow client to read body response
                            this.OnReceivingBody(this.response);

                            this.remaining = (this.size - this.parsed);
                        }

                        break;
                    default:
                        break;
                }

                // HTTP response malfomerd, break
                if (this.result == HttpResponseParserResult.Malformed)
                {
                    this.response = null;
                    break;
                }
            }

            return this.result;
        }

        /// <summary>
        /// Called for reading response body by client
        /// </summary>
        /// <param name="buffer">Destination client buffer</param>
        /// <param name="offset">Offset to start reading</param>
        /// <param name="count">Number of bytes to read</param>
        /// <returns>Number of bytes read</returns>
        private int ReadBody(byte[] buffer, int offset, int count)
        {
            int readBytes = 0;

            // we can read at most bytes inside buffer (remaining)
            readBytes = (this.bodyRemaining <= this.remaining) ? this.bodyRemaining : this.remaining;

            // if client want to read less then remaining bytes
            if (count < readBytes)
                readBytes = count;

            // copy bytes to client buffer
            Array.Copy(this.buffer, this.parsed, buffer, offset, readBytes);
            
            this.parsed += readBytes;

            // body chunked
            if (this.isChunked)
            {
                // if read all chunk, consider CR LF ending chunk
                if (readBytes == this.bodyRemaining)
                {
                    this.parsed += HttpBase.CRLF.Length;
                    // waiting for another chunk
                    this.bodyRemaining = 0;
                }
                // not read entire current chunk
                else
                {
                    this.bodyRemaining -= readBytes;
                }
            }
            else
            {
                this.bodyRemaining -= readBytes;

                // body and parsing end
                if (this.bodyRemaining == 0)
                {
                    this.result = HttpResponseParserResult.Completed;
                    this.state = HttpResponseParserState.ResponseLine;
                }
            }

            return readBytes;
        }

        /// <summary>
        /// Raise receving body event
        /// </summary>
        /// <param name="httpResp">HTTP response for reading body</param>
        private void OnReceivingBody(HttpResponse httpResp)
        {
            if (this.ReceivingBody != null)
                this.ReceivingBody(httpResp);
        }
    }

    /// <summary>
    /// HTTP response parser states
    /// </summary>
    internal enum HttpResponseParserState
    {
        /// <summary>
        /// Receiving response line
        /// </summary>
        ResponseLine,

        /// <summary>
        /// Receiving headers
        /// </summary>
        Headers,

        /// <summary>
        /// Receiving body
        /// </summary>
        Body
    }

    /// <summary>
    /// HTTP line states
    /// </summary>
    internal enum HttpResponseLineState
    {
        /// <summary>
        /// Idle
        /// </summary>
        Idle,

        /// <summary>
        /// Waiting for CR
        /// </summary>
        CarriageReturn,

        /// <summary>
        /// End line (LF recognized)
        /// </summary>
        End
    }

    /// <summary>
    /// HTTP response parser result
    /// </summary>
    internal enum HttpResponseParserResult
    {
        /// <summary>
        /// Completed response
        /// </summary>
        Completed,

        /// <summary>
        /// Not completed response
        /// </summary>
        NotCompleted,

        /// <summary>
        /// Malformed response
        /// </summary>
        Malformed
    }
}
