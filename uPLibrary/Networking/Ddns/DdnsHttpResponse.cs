using System;
using System.Collections;
using Microsoft.SPOT;

namespace uPLibrary.Networking
{
    /// <summary>
    /// Parser for HTTP response from Dynamic Dns service provider
    /// </summary>
    internal class DdnsHttpResponse
    {
        // const string separator
        private const char CR = '\r';
        private const char LF = '\n';

        /// <summary>
        /// HTTP Response Headers
        /// </summary>
        public Hashtable Headers { get; set; }

        /// <summary>
        /// HTTP Response Code
        /// </summary>
        public string ResponseCode { get; set; }

        /// <summary>
        /// HTTP Response Body
        /// </summary>
        public string Body { get; set; }

        internal DdnsHttpResponse()
        {
            this.Headers = new Hashtable();
        }

        /// <summary>
        /// Parse Http response string and return a DdnsHttpResponse object
        /// </summary>
        /// <param name="response">Http response string</param>
        /// <returns>DdnsHttpResponse object</returns>
        internal static DdnsHttpResponse Parse(string response)
        {
            DdnsHttpResponse httpResponse = new DdnsHttpResponse();
            int i = 0;

            // split response lines on line feed
            string[] lines = response.Split(LF);
            // trim line on carriage return
            lines[i] = lines[i].TrimEnd(CR);

            // headers end with empty string
            while (lines[i] != String.Empty)
            {
                int separatorIndex = lines[i].IndexOf(":");

                // first line contains Http response code
                if (i == 0)
                    httpResponse.ResponseCode = lines[i];
                // found header-value separator
                else if (separatorIndex != -1)
                    httpResponse.Headers.Add(lines[i].Substring(0, separatorIndex), lines[i].Substring(separatorIndex + 1).Trim());
                
                i++;
                // trim end carriage return of each line
                lines[i] = lines[i].TrimEnd(CR);
            }

            // next line (body start)
            i++;

            // content length specified
            if (httpResponse.Headers.Contains("Content-Length"))
            {
                httpResponse.Body = lines[i].TrimEnd(CR).Substring(0, Convert.ToInt32(httpResponse.Headers["Content-Length"].ToString()));
            }
            // transfer encoding specified
            else if (httpResponse.Headers.Contains("Transfer-Encoding"))
            {
                // body chunked
                if (httpResponse.Headers["Transfer-Encoding"].ToString() == "chunked")
                {
                    httpResponse.Body = String.Empty;
                    do
                    {
                        int chunkDim = Convert.ToInt32(lines[i++].TrimEnd(CR), 16);
                        httpResponse.Body += lines[i++].TrimEnd(CR).Substring(0, chunkDim);
                    } while (Convert.ToInt32(lines[i].TrimEnd(CR)) != 0);
                }
            }
            
            return httpResponse;
        }
    }
}
