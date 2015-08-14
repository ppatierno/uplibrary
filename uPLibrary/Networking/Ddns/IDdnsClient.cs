using System;
using System.Net;
using Microsoft.SPOT;

namespace uPLibrary.Networking.Ddns
{
    /// <summary>
    /// Interface for Dynamic Dns Clients
    /// </summary>
    public interface IDdnsClient
    {
        /// <summary>
        /// Current IPAddress
        /// </summary>
        IPAddress IpAddress { get; }

        /// <summary>
        /// Start periodic check/update IP address
        /// </summary>
        void Start();

        /// <summary>
        /// Stop periodic check/update IP address
        /// </summary>
        void Stop();
    }
}
