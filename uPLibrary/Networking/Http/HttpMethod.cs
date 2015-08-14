using System;

namespace uPLibrary.Networking.Http
{
    /// <summary>
    /// HTTP methods for request
    /// </summary>
    public class HttpMethod
    {
        #region Fields...

        // static members for HTTP methods
        private static readonly HttpMethod getMethod;
        private static readonly HttpMethod postMethod;
        private static readonly HttpMethod putMethod;
        private static readonly HttpMethod deleteMethod;
        private static readonly HttpMethod headMethod;
        private static readonly HttpMethod optionsMethod;
        private static readonly HttpMethod traceMethod;
        private static readonly HttpMethod patchMethod;

        // HTTP method string
        private string method;

        #endregion

        #region Properties...

        /// <summary>
        /// GET method
        /// </summary>
        public static HttpMethod Get
        {
            get { return getMethod; }
        }

        /// <summary>
        /// POST method
        /// </summary>
        public static HttpMethod Post
        {
            get { return postMethod; }
        }

        /// <summary>
        /// PUT method
        /// </summary>
        public static HttpMethod Put
        {
            get { return putMethod; }
        }

        /// <summary>
        /// DELETE method
        /// </summary>
        public static HttpMethod Delete
        {
            get { return deleteMethod; }
        }

        /// <summary>
        /// HEAD method
        /// </summary>
        public static HttpMethod Head
        {
            get { return headMethod; }
        }

        /// <summary>
        /// OPTIONS method
        /// </summary>
        public static HttpMethod Options
        {
            get { return optionsMethod; }
        }

        /// <summary>
        /// TRACE method
        /// </summary>
        public static HttpMethod Trace
        {
            get { return traceMethod; }
        }

        /// <summary>
        /// PATCH method
        /// </summary>
        public static HttpMethod Patch
        {
            get { return patchMethod; }
        }

        #endregion

        /// <summary>
        /// Static constructor
        /// </summary>
        static HttpMethod()
        {
            getMethod = new HttpMethod("GET");
            postMethod = new HttpMethod("POST");
            putMethod = new HttpMethod("PUT");
            deleteMethod = new HttpMethod("DELETE");
            headMethod = new HttpMethod("HEAD");
            optionsMethod = new HttpMethod("OPTIONS");
            traceMethod = new HttpMethod("TRACE");
            patchMethod = new HttpMethod("PATCH");
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="method">HTTP method</param>
        public HttpMethod(string method)
        {
            this.method = method;
        }

        /// <summary>
        /// Equality between this instance and other HttpMethod instance
        /// </summary>
        /// <param name="other">Other HttpMethod instance</param>
        /// <returns>Equality or not</returns>
        public bool Equals(HttpMethod other)
        {
            if (other == null)
                return false;

            return (Object.ReferenceEquals(this.method, other.method) || (String.Compare(this.method, other.method) == 0));
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as HttpMethod);
        }

        public override int GetHashCode()
        {
            return this.method.GetHashCode();
        }

        public override string ToString()
        {
            return this.method;
        }
    }
}
