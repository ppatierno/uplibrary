using System;

namespace uPLibrary.Networking.Http
{
    /// <summary>
    /// HTTP response body
    /// </summary>
    public class HttpResponseBody : IHttpBody
    {
        public WriteCallback Write
        {
            get
            {
                throw new NotImplementedException("You cannot write on a HTTP response body");
            }
            set
            {
                throw new NotImplementedException("You cannot write on a HTTP response body");
            }
        }

        public ReadCallback Read { get; set; }
    }
}
