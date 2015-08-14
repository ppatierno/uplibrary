using System;
using Microsoft.SPOT;

namespace uPLibrary.Networking.Ddns
{
    /// <summary>
    /// Factory class for Dynamic Dns Clients
    /// </summary>
    public static class DdnsClientFactory
    {
        /// <summary>
        /// Return the requested DdnsClient instance based on specified Ddns Service Provider
        /// </summary>
        /// <param name="ddnsServiceProvider">Ddns Service Provider for create the right DdnsClient instance</param>
        /// <param name="ddnsConfig">Ddns configuration information</param>
        /// <returns>Concrete DdnsClient instance</returns>
        public static DdnsClient GetDdnsClient(DdnsServiceProvider ddnsServiceProvider, DdnsConfig ddnsConfig)
        {
            DdnsClient ddnsClient = null;

            switch (ddnsServiceProvider)
            {
                case DdnsServiceProvider.NoIp:
                    ddnsClient = new DdnsNoIpClient(ddnsConfig);
                    break;
                case DdnsServiceProvider.DynDns:
                    ddnsClient = new DdnsDynDnsClient(ddnsConfig);
                    break;
                default:
                    break;
            }

            return ddnsClient;
        }
    }

    /// <summary>
    /// Dynamic Dns Service Provider availables for the factory
    /// </summary>
    public enum DdnsServiceProvider
    {
        /// <summary>
        /// No-Ip (http://www.no-ip.com/)
        /// </summary>
        NoIp,

        /// <summary>
        /// DynDns (http://dyn.com/)
        /// </summary>
        DynDns
    }
}
