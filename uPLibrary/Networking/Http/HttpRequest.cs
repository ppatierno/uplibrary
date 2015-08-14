using System;
using System.Collections;
using System.Text;

namespace uPLibrary.Networking.Http
{
    /// <summary>
    /// Class for HTTP request
    /// </summary>
    public class HttpRequest : HttpBase
    {

        /// <summary>
        /// HTTP request method
        /// </summary>
        public HttpMethod Method { get; set; }

        /// <summary>
        /// Request URI
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public HttpRequest()
            : base()
        {
            this.Body = new HttpRequesteBody();
        }

        public override string ToString()
        {
            // extract query string
            string queryString = this.Uri.AbsoluteUri.Substring(this.Uri.AbsoluteUri.IndexOf(this.Uri.AbsolutePath) + this.Uri.AbsolutePath.Length);

#if !MF_FRAMEWORK_VERSION_V4_1
            StringBuilder sBuilder = new StringBuilder();           
            
            // request line
            sBuilder.Append(this.Method).Append(" ");
            sBuilder.Append(this.Uri.AbsolutePath);
            sBuilder.Append(queryString).Append(" ");
            sBuilder.Append(this.HttpProtocol);
            sBuilder.Append(HttpBase.CRLF);

            // add header lines
            foreach (DictionaryEntry item in this.Headers)
            {
                sBuilder.Append(item.Key).Append(": ");
                sBuilder.Append(item.Value);
                sBuilder.Append(HttpBase.CRLF);
            }
            sBuilder.Append(HttpBase.CRLF);

            return sBuilder.ToString();
#else
            // request line
            string sBuilder = this.Method + " " + this.Uri.AbsolutePath + " " + this.HttpProtocol + HttpBase.CRLF;

            // add header lines
            foreach (DictionaryEntry item in this.Headers)
            {
                sBuilder += item.Key + ": " + item.Value + HttpBase.CRLF;
            }
            sBuilder += HttpBase.CRLF;

            return sBuilder;
#endif
        }
    }
}
