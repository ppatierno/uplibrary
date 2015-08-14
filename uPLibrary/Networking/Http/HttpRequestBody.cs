using System;

namespace uPLibrary.Networking.Http
{
    /// <summary>
    /// HTTP request body
    /// </summary>
    public class HttpRequesteBody : IHttpBody
    {
        public WriteCallback Write { get; set; }

        public ReadCallback Read
        {
            get
            {
                throw new NotImplementedException("You cannot read on a HTTP request body");
            }
            set
            {
                throw new NotImplementedException("You cannot read on a HTTP request body");
            }
        }
    }
}