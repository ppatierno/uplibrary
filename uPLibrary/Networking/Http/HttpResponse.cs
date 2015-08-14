using System;

namespace uPLibrary.Networking.Http
{
    /// <summary>
    /// Class for HTTP response
    /// </summary>
    public class HttpResponse : HttpBase
    {
        /// <summary>
        /// HTTP response status code
        /// </summary>
        public HttpStatusCode StatusCode { get; internal set; }

        /// <summary>
        /// HTTP reason phrase
        /// </summary>
        public string ReasonPhrase { get; internal set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public HttpResponse()
            : base()
        {
            this.Body = new HttpResponseBody();
        }
    }
}
