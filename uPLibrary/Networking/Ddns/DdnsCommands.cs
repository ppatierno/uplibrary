using System;
using Microsoft.SPOT;

namespace uPLibrary.Networking.Ddns
{
    /// <summary>
    /// Base class for Dynamic Dns Client commands
    /// </summary>
    public abstract class DdnsBaseCommand
    {
        internal const string CRLF = "\r\n";

        /// <summary>
        /// Path for GET request
        /// </summary>
        internal string Path { get; set; }

        /// <summary>
        /// Host server name to send request
        /// </summary>
        internal string Host { get; set; }
    }

    /// <summary>
    /// Check IP address command class
    /// </summary>
    public class DdnsCheckIpCommand : DdnsBaseCommand
    {
        public override string ToString()
        {
            if (this.Path == null)
                throw new ArgumentNullException("Path is null");

            if (this.Host == null)
                throw new ArgumentNullException("Host is null");

            // make string for GET request
            string request = "GET " + this.Path + " HTTP/1.1" + CRLF;
            request += "Host: " + this.Host + CRLF;
            request += CRLF;
            return request;
        }
    }

    /// <summary>
    /// Update Ip address command class
    /// </summary>
    public class DdnsUpdateIpCommand : DdnsBaseCommand
    {
        /// <summary>
        /// Hostname for which update IP address
        /// </summary>
        internal string HostName { get; set; }

        /// <summary>
        /// IP address to update
        /// </summary>
        internal string IpAddress { get; set; }

        /// <summary>
        /// Authorization header with username and password (Base64 encoding)
        /// </summary>
        internal string Authorization { get; set; }

        /// <summary>
        /// User agent
        /// </summary>
        internal string UserAgent { get; set; }

        public override string ToString()
        {
            if (this.Path == null)
                throw new ArgumentNullException("Path is null");

            if (this.Host == null)
                throw new ArgumentNullException("Host is null");

            if (this.HostName == null)
                throw new ArgumentNullException("HostName is null");

            if (this.Authorization == null)
                throw new ArgumentNullException("Authorization is null");

            if (this.UserAgent == null)
                throw new ArgumentNullException("UserAgent is null");

            // make string for GET request
            string request = "GET " + this.Path + "?hostname=" + this.HostName;

            // if IP address is specified
            if (this.IpAddress != null)
                request += "&myip=" + this.IpAddress;

            request += " HTTP/1.1" + CRLF;
            request += "Host: " + this.Host + CRLF;
            request += "Authorization: Basic " + this.Authorization + CRLF;
            request += "User-Agent: " + this.UserAgent + CRLF;
            request += CRLF;
            return request;
        }
    }
}
