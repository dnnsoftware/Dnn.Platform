#region Apache License
//
// Licensed to the Apache Software Foundation (ASF) under one or more 
// contributor license agreements. See the NOTICE file distributed with
// this work for additional information regarding copyright ownership. 
// The ASF licenses this file to you under the Apache License, Version 2.0
// (the "License"); you may not use this file except in compliance with 
// the License. You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Net;

namespace log4net.Util.TypeConverters
{
	/// <summary>
	/// Supports conversion from string to <see cref="IPAddress"/> type.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Supports conversion from string to <see cref="IPAddress"/> type.
	/// </para>
	/// </remarks>
	/// <seealso cref="ConverterRegistry"/>
	/// <seealso cref="IConvertFrom"/>
	/// <author>Nicko Cadell</author>
	internal class IPAddressConverter : IConvertFrom 
	{
		#region Implementation of IConvertFrom

		/// <summary>
		/// Can the source type be converted to the type supported by this object
		/// </summary>
		/// <param name="sourceType">the type to convert</param>
		/// <returns>true if the conversion is possible</returns>
		/// <remarks>
		/// <para>
		/// Returns <c>true</c> if the <paramref name="sourceType"/> is
		/// the <see cref="String"/> type.
		/// </para>
		/// </remarks>
		public bool CanConvertFrom(Type sourceType) 
		{
			return (sourceType == typeof(string));
		}

		/// <summary>
		/// Overrides the ConvertFrom method of IConvertFrom.
		/// </summary>
		/// <param name="source">the object to convert to an IPAddress</param>
		/// <returns>the IPAddress</returns>
		/// <remarks>
		/// <para>
		/// Uses the <see cref="IPAddress.Parse"/> method to convert the
		/// <see cref="String"/> argument to an <see cref="IPAddress"/>.
		/// If that fails then the string is resolved as a DNS hostname.
		/// </para>
		/// </remarks>
		/// <exception cref="ConversionNotSupportedException">
		/// The <paramref name="source"/> object cannot be converted to the
		/// target type. To check for this condition use the <see cref="CanConvertFrom"/>
		/// method.
		/// </exception>
		public object ConvertFrom(object source) 
		{
			string str = source as string;
			if (str != null && str.Length > 0) 
			{
				try
				{
#if NET_2_0 || NETCF_2_0

#if !NETCF_2_0
					// Try an explicit parse of string representation of an IPAddress (v4 or v6)
					IPAddress result;
					if (IPAddress.TryParse(str, out result))
					{
						return result;
					}
#endif

					// Try to resolve via DNS. This is a blocking call. 
					// GetHostEntry works with either an IPAddress string or a host name
					IPHostEntry host = Dns.GetHostEntry(str);
					if (host != null && 
						host.AddressList != null && 
						host.AddressList.Length > 0 &&
						host.AddressList[0] != null)
					{
						return host.AddressList[0];
					}
#else
					// Before .NET 2 we need to try to parse the IPAddress from the string first

					// Check if the string only contains IP address valid chars
					if (str.Trim(validIpAddressChars).Length == 0)
					{
						try
						{
							// try to parse the string as an IP address
							return IPAddress.Parse(str);
						}
						catch(FormatException)
						{
							// Ignore a FormatException, try to resolve via DNS
						}
					}

					// Try to resolve via DNS. This is a blocking call.
					IPHostEntry host = Dns.GetHostByName(str);
					if (host != null && 
						host.AddressList != null && 
						host.AddressList.Length > 0 &&
						host.AddressList[0] != null)
					{
						return host.AddressList[0];
					}
#endif
				}
				catch(Exception ex)
				{
					throw ConversionNotSupportedException.Create(typeof(IPAddress), source, ex);
				}
			}
			throw ConversionNotSupportedException.Create(typeof(IPAddress), source);
		}

		#endregion

		/// <summary>
		/// Valid characters in an IPv4 or IPv6 address string. (Does not support subnets)
		/// </summary>
		private static readonly char[] validIpAddressChars = {'0','1','2','3','4','5','6','7','8','9','a','b','c','d','e','f','A','B','C','D','E','F','x','X','.',':','%'};
	}
}
