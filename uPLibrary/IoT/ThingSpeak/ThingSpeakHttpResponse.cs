using System;
using System.Collections;
using Microsoft.SPOT;

namespace uPLibrary.IoT.ThingSpeak
{
    /// <summary>
    /// Parser for HTTP response from ThingSpeak service
    /// </summary>
    internal class ThingSpeakHttpResponse
    {
        // const string separator
        private const char CR = '\r';
        private const char LF = '\n';
        private const string CRLF = "\r\n";

        /// <summary>
        /// HTTP Response Headers
        /// </summary>
        public Hashtable Headers { get; set; }

        /// <summary>
        /// HTTP Response Code
        /// </summary>
        public string ResponseCode { get; set; }

        /// <summary>
        /// HTTP Status Code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// HTTP Response Body
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        internal ThingSpeakHttpResponse()
        {
            this.Headers = new Hashtable();
        }

        /// <summary>
        /// Parse Http response string and return a ThingSpeakHttpResponse object
        /// </summary>
        /// <param name="response">Http response string</param>
        /// <returns>ThingSpeakHttpResponse object</returns>
        internal static ThingSpeakHttpResponse Parse(string response)
        {
            ThingSpeakHttpResponse httpResponse = new ThingSpeakHttpResponse();
            int i = 0;
            int headerSize = 0;

            // split response lines on line feed
            string[] lines = response.Split(LF);
            // trim line on carriage return
            lines[i] = lines[i].TrimEnd(CR);

            // headers end with empty string
            while (lines[i] != String.Empty)
            {
                // calculate header size
                headerSize += lines[i].Length + CRLF.Length;

                int separatorIndex = lines[i].IndexOf(":");

                // first line contains Http response code
                if (i == 0)
                {
                    httpResponse.ResponseCode = lines[i];
                    httpResponse.StatusCode = Convert.ToInt32(httpResponse.ResponseCode.Substring(httpResponse.ResponseCode.IndexOf(' ') + 1, 3));
                }
                // found header-value separator
                else if (separatorIndex != -1)
                    httpResponse.Headers.Add(lines[i].Substring(0, separatorIndex), lines[i].Substring(separatorIndex + 1).Trim());
                
                i++;
                // trim end carriage return of each line
                lines[i] = lines[i].TrimEnd(CR);  
            }

            // next line (body start)
            i++;

            // set start of body inside response
            int bodyOffset = headerSize + CRLF.Length;
            // get body from entire response
            string body = response.Substring(bodyOffset);

            // content length specified
            if (httpResponse.Headers.Contains("Content-Length"))
            {
                httpResponse.Body = body.Substring(0, Convert.ToInt32(httpResponse.Headers["Content-Length"].ToString()));
            }
            // transfer encoding specified
            else if (httpResponse.Headers.Contains("Transfer-Encoding"))
            {
                // body chunked
                if (httpResponse.Headers["Transfer-Encoding"].ToString() == "chunked")
                {
                    httpResponse.Body = String.Empty;
                    int chunkDim = 0;
                    int currBodyOffset = 0;
                    do
                    {
                        // get chunk dimension
                        chunkDim = Convert.ToInt32(body.Substring(0, body.IndexOf(CRLF)), 16);
                        // get the following body chunk
                        if (chunkDim != 0)
                        {
                            // set offset after chunk dimension
                            currBodyOffset = body.IndexOf(CRLF) + CRLF.Length;
                            // get the body chunk
                            httpResponse.Body += body.Substring(currBodyOffset, chunkDim);
                            // update body part to analize
                            body = body.Substring(currBodyOffset + chunkDim + CRLF.Length);
                        }
                    } while (chunkDim != 0);
                    // trim last CRLF of body
                    httpResponse.Body = httpResponse.Body.TrimEnd(LF).TrimEnd(CR);
                }
            }
            
            return httpResponse;
        }
    }
}
