using System;

namespace log4net.Util.TypeConverters
{
	internal class TypeConverter : IConvertFrom
	{
		public TypeConverter()
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
				throw ConversionNotSupportedException.Create(typeof(Type), source);
			}
			return SystemInfo.GetTypeFromString(str, true, true);
		}
	}
}