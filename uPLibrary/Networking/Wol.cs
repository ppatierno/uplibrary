using System;
using Microsoft.SPOT;
using System.Net.Sockets;
using System.Net;

namespace uPLibrary.Networking
{
    /// <summary>
    /// Implements Wake on Lan functionality
    /// </summary>
    public static class Wol
    {
        // magic packet size (102 bytes)
        private const int MAGIC_PACKET_SIZE = 102;
        // magic packet header size (6 bytes) all value 0xFF
        private const int MAGIC_PACKET_HEADER_SIZE = 6;
        // number of ripetitions of mac address into magic packet
        private const int MAGIC_PACKET_MAC_ADDR_RIPETITION = 16;
        // mac address dimension (6 bytes)
        private const int MAC_ADDR_SIZE = 6;
        // broadcast ip address
        private const string BROADCAST_IP_ADDRESS = "255.255.255.255";

        // possible wake on lan UDP port
        private const int WOL_PORT = 7;
        private const int WOL_PORT2 = 9;
        
        /// <summary>
        /// Send the magic packet on the broadcast IP address for wake on LAN functionality
        /// </summary>
        /// <param name="macAddress">MAC address destination for Wake on LAN</param>
        public static void Wake(byte[] macAddress)
        {
            // create UDP socket and set for broadcast
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                        
            // set endpoint with broadcast ip address and WOL port
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(BROADCAST_IP_ADDRESS), WOL_PORT);
            
            // add magic packet header
            byte[] magicPacket = new byte[MAGIC_PACKET_SIZE];
            for (int i = 0; i < MAGIC_PACKET_HEADER_SIZE; i++)
                magicPacket[i] = 0xFF;

            // write 16 copies of mac address into magic packet
            int magicPacketIdx = MAGIC_PACKET_HEADER_SIZE;
            for (int i = 0; i < MAGIC_PACKET_MAC_ADDR_RIPETITION; i++)
            {
                for (int j = 0; j < MAC_ADDR_SIZE; j++)
                {
                    magicPacket[magicPacketIdx] = macAddress[j];
                    magicPacketIdx++;
                }
            }

            // send magic packet and close
            socket.SendTo(magicPacket, ipEndPoint);
            socket.Close();
        }
    }
}
