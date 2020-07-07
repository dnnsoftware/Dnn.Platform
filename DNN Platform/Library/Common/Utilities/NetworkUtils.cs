// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utils
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Web;

    /// <summary>
    /// Enumration of IP AddressTyes.
    /// </summary>
    public enum AddressType
    {
        IPv4,
        IPv6,
    }

    /// <summary>
    /// Utility functions for network information.
    /// </summary>
    public class NetworkUtils
    {
        /// <summary>
        /// Gets the address.
        /// </summary>
        /// <param name="Host">The host.</param>
        /// <param name="AddressFormat">The address format.</param>
        /// <returns>Returns IP address.</returns>
        /// <remarks><seealso cref="AddressType"></seealso></remarks>
        public static string GetAddress(string Host, AddressType AddressFormat)
        {
            AddressFamily addrFamily = AddressFamily.InterNetwork;
            switch (AddressFormat)
            {
                case AddressType.IPv4:
                    addrFamily = AddressFamily.InterNetwork;
                    break;
                case AddressType.IPv6:
                    addrFamily = AddressFamily.InterNetworkV6;
                    break;
            }

            IPHostEntry IPE = Dns.GetHostEntry(Host);
            if (Host != IPE.HostName)
            {
                IPE = Dns.GetHostEntry(IPE.HostName);
            }

            foreach (IPAddress IPA in IPE.AddressList)
            {
                if (IPA.AddressFamily == addrFamily)
                {
                    return IPA.ToString();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Convert IP address to long integer.
        /// </summary>
        /// <param name="ip">The ip address.</param>
        /// <returns>IP Address in long.</returns>
        public static long IPtoLong(IPAddress ip)
        {
            // convert IP to number
            byte[] addressBytes = ip.GetAddressBytes();

            // get the octets
            long addr = 0;

            // accumulator for address
            for (int x = 0; x <= 3; x++)
            {
                addr = addr | (Convert.ToInt64(addressBytes[x]) << ((3 - x) * 8));
            }

            return addr;
        }

        /// <summary>
        /// Longs to ip address.
        /// </summary>
        /// <param name="ip">The ip address.</param>
        /// <returns>IP Number as formatted string.</returns>
        public static string LongToIp(long ip)
        {
            // convert number back to IP
            var ipByte = new byte[4];

            // 4 octets
            string addr = string.Empty;

            // accumulator for address
            long mask8 = MaskFromCidr(8);

            // create eight bit mask

            // get the octets
            for (int x = 0; x <= 3; x++)
            {
                ipByte[x] = Convert.ToByte((ip & mask8) >> ((3 - x) * 8));
                mask8 = mask8 >> 8;
                addr += ipByte[x].ToString() + ".";

                // add current octet to string
            }

            return addr.TrimEnd('.');
        }

        /// <summary>
        /// Formats as cidr.
        /// </summary>
        /// <param name="startIP">The start ip.</param>
        /// <param name="subnetMask">The subnet mask.</param>
        /// <returns>Classless Inter-Domain Routing.</returns>
        public static string FormatAsCidr(string startIP, string subnetMask)
        {
            if (string.IsNullOrEmpty(subnetMask))
            {
                return startIP;
            }

            IPAddress ipAddress = IPAddress.Parse(startIP);
            IPAddress mask = IPAddress.Parse(subnetMask);

            long ipL = IPtoLong(ipAddress);
            long maskL = IPtoLong(mask);

            // Convert  Mask to CIDR(1-30)
            long oneBit = 0x80000000L;
            int cidr = 0;

            for (int x = 31; x >= 0; x += -1)
            {
                if ((maskL & oneBit) == oneBit)
                {
                    cidr += 1;
                }
                else
                {
                    break;
                }

                oneBit = oneBit >> 1;
            }

            string answer = LongToIp(ipL & maskL) + " /" + cidr.ToString();
            return answer;
        }

        /// <summary>
        /// Network2s the ip range.
        /// </summary>
        /// <param name="sNetwork">The network name.</param>
        /// <param name="startIP">The start ip.</param>
        /// <param name="endIP">The end ip.</param>
        public static void Network2IpRange(string sNetwork, out uint startIP, out uint endIP)
        {
            try
            {
                string[] elements = sNetwork.Split(new[] { '/' });

                uint ip = IP2Int(elements[0]);
                int bits = Convert.ToInt32(elements[1]);

                uint mask = ~(0xffffffff >> bits);

                uint network = ip & mask;
                uint broadcast = network + ~mask;

                uint usableIps = (bits > 30) ? 0 : (broadcast - network - 1);

                if (usableIps <= 0)
                {
                    startIP = endIP = 0;
                }
                else
                {
                    startIP = network + 1;
                    endIP = broadcast - 1;
                }
            }
            catch (Exception)
            {
                // catch case where IP cannot be resolved such as when debugger is attached
                startIP = 0;
                endIP = 0;
            }
        }

        /// <summary>
        /// Convert IP to Integer.
        /// </summary>
        /// <param name="ipNumber">The ip number.</param>
        /// <returns>IP number as integer.</returns>
        public static uint IP2Int(string ipNumber)
        {
            uint ip = 0;
            string[] elements = ipNumber.Split(new[] { '.' });
            if (elements.Length == 4)
            {
                ip = Convert.ToUInt32(elements[0]) << 24;
                ip += Convert.ToUInt32(elements[1]) << 16;
                ip += Convert.ToUInt32(elements[2]) << 8;
                ip += Convert.ToUInt32(elements[3]);
            }

            return ip;
        }

        /// <summary>
        /// Determines whether ip is in range.
        /// </summary>
        /// <param name="currentIP">The current ip.</param>
        /// <param name="startIP">The start ip.</param>
        /// <param name="subnetmask">The subnetmask.</param>
        /// <returns>True or False.</returns>
        public static bool IsIPInRange(string currentIP, string startIP, string subnetmask)
        {
            try
            {
                // handle case where local adapter is localhost
                if (currentIP == "::1")
                {
                    currentIP = "127.0.0.1";
                }

                // handle case where we are matching on a single IP
                if (string.IsNullOrEmpty(subnetmask))
                {
                    if (currentIP == startIP)
                    {
                        return true;
                    }
                }

                // handle case where we have to build a CIDR, convert to an IP range and compare
                string cidr = FormatAsCidr(startIP, subnetmask);
                uint fromIP, toIP;
                Network2IpRange(cidr, out fromIP, out toIP);
                IPAddress currentIPAsInt = IPAddress.Parse(currentIP);
                long myip = IPtoLong(currentIPAsInt);
                if (myip >= fromIP & myip <= toIP)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                // catch case where IP cannot be resolved such as when debugger is attached
                return false;
            }

            return false;
        }

        /// <summary>
        /// Gets the Client IP address of the current request, from server variables if available, otherwise returns Request.UserHostAddress.
        /// </summary>
        /// <param name="request">The current http request.</param>
        /// <returns>The current client ip address.</returns>
        public static string GetClientIpAddress(HttpRequest request)
        {
            var ipAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"]?.Split(',').FirstOrDefault();

            // If there is no proxy, get the standard remote address
            if (string.IsNullOrWhiteSpace(ipAddress) || ipAddress.Equals("unknown", StringComparison.OrdinalIgnoreCase))
            {
                ipAddress = request.UserHostAddress;
            }

            return ipAddress;
        }

        /// <summary>
        /// Masks from cidr.
        /// </summary>
        /// <param name="cidr">The Classless Inter-Domain Routing (cidr).</param>
        /// <returns></returns>
        private static long MaskFromCidr(int cidr)
        {
            return Convert.ToInt64(Math.Pow(2, 32 - cidr) - 1) ^ 4294967295L;
        }
    }
}
