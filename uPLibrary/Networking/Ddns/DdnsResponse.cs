using System;
using Microsoft.SPOT;

namespace uPLibrary.Networking.Ddns
{
    /// <summary>
    /// Response from Dynamic Dns Service Provider
    /// </summary>
    public class DdnsResponse
    {
        private const string Good = "good";
        private const string NoChg = "nochg";
        private const string NoHost = "nohost";
        private const string BadAuth = "badauth";
        private const string BadAgent = "badagent";
        private const string Donator = "!donator";
        private const string Abuse = "abuse";
        private const string NineOneOne = "911";
        private const string NotFqdn = "notfqdn";
        private const string NumHost = "numhost";
        private const string DnsErr = "dnserr";

        /// <summary>
        /// Response code from Dynamic Dns service provider
        /// </summary>
        public DdnsResponseCode Code { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="responseCode">Response code string from Dynamic Dns service provider</param>
        internal DdnsResponse(string responseCode)
        {
            switch (responseCode)
            {
                case Good:
                    this.Code = DdnsResponseCode.Good;
                    break;
                case NoChg:
                    this.Code = DdnsResponseCode.NoChg;
                    break;
                case NoHost:
                    this.Code = DdnsResponseCode.NoHost;
                    break;
                case BadAuth:
                    this.Code = DdnsResponseCode.BadAuth;
                    break;
                case BadAgent:
                    this.Code = DdnsResponseCode.BadAgent;
                    break;
                case Donator:
                    this.Code = DdnsResponseCode.Donator;
                    break;
                case Abuse:
                    this.Code = DdnsResponseCode.Abuse;
                    break;
                case NineOneOne:
                    this.Code = DdnsResponseCode.NineOneOne;
                    break;
                case NotFqdn:
                    this.Code = DdnsResponseCode.NotFqDn;
                    break;
                case NumHost:
                    this.Code = DdnsResponseCode.NumHost;
                    break;
                case DnsErr:
                    this.Code = DdnsResponseCode.DnsErr;
                    break;
            }
        }
    }

    /// <summary>
    /// Response codes for Dynamic Dns Service Provider
    /// </summary>
    public enum DdnsResponseCode
    {
        /// <summary>
        /// Update successful
        /// </summary>
        Good,

        /// <summary>
        /// Update successful but IP address is not changed
        /// </summary>
        NoChg,

        /// <summary>
        /// The hostname specified does not exist in this user account 
        /// </summary>
        NoHost,

        /// <summary>
        /// The username and password pair do not match a real user
        /// </summary>
        BadAuth,

        /// <summary>
        /// The user agent was not sent or HTTP method is not permitted
        /// </summary>
        BadAgent,

        /// <summary>
        /// An option available only to credited users (such as offline URL) was specified, but the user is not a credited user
        /// </summary>
        Donator,

        /// <summary>
        /// The hostname specified is blocked for update abuse
        /// </summary>
        Abuse,

        /// <summary>
        /// There is a problem or scheduled maintenance on our side
        /// </summary>
        NineOneOne,

        /// <summary>
        /// The hostname specified is not a fully-qualified domain name
        /// </summary>
        NotFqDn,

        /// <summary>
        /// Too many hosts (more than 20) specified in an update
        /// </summary>
        NumHost,

        /// <summary>
        /// DNS error encountered
        /// </summary>
        DnsErr
    }
}
