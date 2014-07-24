using System;
using System.Text;

namespace log4net.Util.TypeConverters
{
	internal class EncodingConverter : IConvertFrom
	{
		public EncodingConverter()
		{
		}

		public bool CanConvertFrom(Type sourceType)
		{
			return sourceType == typeof(string);
		}

		public object ConvertFrom(object source)
		{
			string str = source as string;
			if (str == null)
			{
				throw ConversionNotSupportedException.Create(typeof(Encoding), source);
			}
			return Encoding.GetEncoding(str);
		}
	}
}