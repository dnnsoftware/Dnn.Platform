using System;

namespace log4net.Util.TypeConverters
{
	internal class BooleanConverter : IConvertFrom
	{
		public BooleanConverter()
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
				throw ConversionNotSupportedException.Create(typeof(bool), source);
			}
			return bool.Parse(str);
		}
	}
}