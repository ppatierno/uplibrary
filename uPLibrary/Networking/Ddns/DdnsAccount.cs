using System;
using Microsoft.SPOT;

namespace uPLibrary.Networking.Ddns
{
    /// <summary>
    /// Dynamic Dns Account struct to check/update
    /// </summary>
    public struct DdnsAccount
    {
        /// <summary>
        /// Hostname
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }
    }
}
