using System;
using Microsoft.SPOT;
using System.Net;

namespace uPLibrary.Networking.Ddns
{
    /// <summary>
    /// Dynamic Dns Configuration
    /// </summary>
    public struct DdnsConfig
    {
        /// <summary>
        /// Period for check/update timer
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// Account information
        /// </summary>
        public DdnsAccount Account { get; set; }

        /// <summary>
        /// SSL actived or not
        /// </summary>
        public bool SSL { get; set; }
    }
}
