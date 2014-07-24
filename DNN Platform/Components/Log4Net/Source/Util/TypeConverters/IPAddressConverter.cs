using System;
using System.Net;

namespace log4net.Util.TypeConverters
{
	internal class IPAddressConverter : IConvertFrom
	{
		private readonly static char[] validIpAddressChars;

		public bool IsIPv6Supported
		{
			get
			{
				return false;
			}
		}

		static IPAddressConverter()
		{
			IPAddressConverter.validIpAddressChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'A', 'B', 'C', 'D', 'E', 'F', 'x', 'X', '.', ':', '%' };
		}

		public IPAddressConverter()
		{
		}

		public bool CanConvertFrom(Type sourceType)
		{
			return sourceType == typeof(string);
		}

		public object ConvertFrom(object source)
		{
			IPAddress pAddress;
			object addressList;
			string str = source as string;
			if (str != null && str.Length > 0)
			{
				try
				{
					if (!IPAddress.TryParse(str, out pAddress))
					{
						IPHostEntry hostEntry = Dns.GetHostEntry(str);
						if (hostEntry == null || hostEntry.AddressList == null || (int)hostEntry.AddressList.Length <= 0 || hostEntry.AddressList[0] == null)
						{
							throw ConversionNotSupportedException.Create(typeof(IPAddress), source);
						}
						else
						{
							addressList = hostEntry.AddressList[0];
						}
					}
					else
					{
						addressList = pAddress;
					}
				}
				catch (Exception exception)
				{
					throw ConversionNotSupportedException.Create(typeof(IPAddress), source, exception);
				}
				return addressList;
			}
			throw ConversionNotSupportedException.Create(typeof(IPAddress), source);
		}
	}
}