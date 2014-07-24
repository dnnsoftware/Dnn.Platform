using log4net.Layout;
using System;

namespace log4net.Util.TypeConverters
{
	internal class PatternLayoutConverter : IConvertFrom
	{
		public PatternLayoutConverter()
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
				throw ConversionNotSupportedException.Create(typeof(PatternLayout), source);
			}
			return new PatternLayout(str);
		}
	}
}