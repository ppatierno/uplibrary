using System;
using Microsoft.SPOT;
using System.Net;
using System.Text;
using uPLibrary.Utilities;

namespace uPLibrary.Networking.Ddns
{
    /// <summary>
    /// Dynamic Dns Client class for DynDns service provider
    /// </summary>
    public class DdnsDynDnsClient : DdnsClient
    {
        // URL and port for checking IP address
        private const string DDNS_CHECK_IP_HOST = "checkip.dyndns.org";
        private const int DDNS_CHECK_IP_PORT = 80;
        // URL and ports for updating IP address
        private const string DDNS_UPDATE_IP_HOST = "members.dyndns.org";
        private const int DDNS_UPDATE_IP_PORT = 80;
        private const int DDNS_UPDATE_IP_SSL_PORT = 443;

        public DdnsDynDnsClient(DdnsConfig ddnsConfig)
            : base(ddnsConfig)
        {
            // **** CHECK IP ADDRESS CONFIGURATION ****

            this.checkIpHost = DDNS_CHECK_IP_HOST;

            // get IP address for Ddns check ip hostname and create endpoint
            IPHostEntry hostEntry = Dns.GetHostEntry(this.checkIpHost);
            this.checkIpEndPoint = new IPEndPoint(hostEntry.AddressList[0], DDNS_CHECK_IP_PORT);

            // create command for check IP address
            this.checkIpCmd = new DdnsCheckIpCommand
            {
                Path = DDNS_CHECK_IP_PATH,
                Host = DDNS_CHECK_IP_HOST
            };

            // **** UPDATE IP ADDRESS CONFIGURATION ****
            
            this.updateIpHost = DDNS_UPDATE_IP_HOST;

            // get IP address for Ddns service provider hostname and create endpoint
            hostEntry = Dns.GetHostEntry(this.updateIpHost);
            this.updateIpEndPoint = new IPEndPoint(hostEntry.AddressList[0], this.ddnsConfig.SSL ? DDNS_UPDATE_IP_SSL_PORT : DDNS_UPDATE_IP_PORT);

            // create command for update IP address
            this.updateIpCmd = new DdnsUpdateIpCommand
            {
                Path = DDNS_UPDATE_IP_PATH,
                Host = DDNS_UPDATE_IP_HOST,
                HostName = this.ddnsConfig.Account.Hostname,
                Authorization = Encoder.ToBase64String(Encoding.UTF8.GetBytes(this.ddnsConfig.Account.Username + ":" + this.ddnsConfig.Account.Password)),
                UserAgent = DDNS_CLIENT_USER_AGENT
            };
        }

        protected override DdnsResponse DecodeUpdateIpResponse(string updateIpResp)
        {
            // parse Http response
            DdnsHttpResponse httpResponse = DdnsHttpResponse.Parse(updateIpResp);

            string responseCode = null;

            // extract response code
            if (httpResponse.Body.LastIndexOf(' ') > 0)
                responseCode = httpResponse.Body.Substring(0, httpResponse.Body.LastIndexOf(' '));
            else
                responseCode = httpResponse.Body;

            DdnsResponse ddnsResponse = new DdnsResponse(responseCode);

            // if response is "good" or "nochg", No-Ip service provider returns also IP address
            if ((ddnsResponse.Code == DdnsResponseCode.Good) ||
                (ddnsResponse.Code == DdnsResponseCode.NoChg))
            {
                // ex. good aaa.bbb.ccc.ddd
                //     nochg aaa.bbb.ccc.ddd
                this.ipAddress = IPAddress.Parse(httpResponse.Body.Substring(httpResponse.Body.LastIndexOf(' ') + 1));
            }

            return ddnsResponse;
        }

        protected override IPAddress DecodeCheckIpResponse(string checkedIpResp)
        {
            // extract body from response
            string body = checkedIpResp.Substring(checkedIpResp.IndexOf("<html>"), checkedIpResp.LastIndexOf(DdnsBaseCommand.CRLF) - checkedIpResp.IndexOf("<html>"));
            
            string response = body.Substring(body.IndexOf("Address:") + 9);
            response = response.Substring(0, response.IndexOf('<'));

            return IPAddress.Parse(response);
        }
    }
}
