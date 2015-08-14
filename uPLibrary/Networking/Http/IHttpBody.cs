using System;

namespace uPLibrary.Networking.Http
{
    public delegate void WriteCallback(byte[] buffer, int offset, int count);
    public delegate int ReadCallback(byte[] buffer, int offset, int count);

    /// <summary>
    /// HTTP body interface
    /// </summary>
    public interface IHttpBody
    {
        /// <summary>
        /// Callback for writing request body
        /// </summary>
        WriteCallback Write { get; set; }

        /// <summary>
        /// Callback for reading response body
        /// </summary>
        ReadCallback Read { get; set; }
    }
}
