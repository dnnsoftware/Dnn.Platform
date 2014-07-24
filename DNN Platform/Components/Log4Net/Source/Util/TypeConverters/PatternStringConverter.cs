using System;

namespace log4net.Util.TypeConverters
{
	internal class PatternStringConverter : IConvertTo, IConvertFrom
	{
		public PatternStringConverter()
		{
		}

		public bool CanConvertFrom(Type sourceType)
		{
			return sourceType == typeof(string);
		}

		public bool CanConvertTo(Type targetType)
		{
			return typeof(string).IsAssignableFrom(targetType);
		}

		public object ConvertFrom(object source)
		{
			string str = source as string;
			if (str == null)
			{
				throw ConversionNotSupportedException.Create(typeof(PatternString), source);
			}
			return new PatternString(str);
		}

		public object ConvertTo(object source, Type targetType)
		{
			PatternString patternString = source as PatternString;
			if (patternString == null || !this.CanConvertTo(targetType))
			{
				throw ConversionNotSupportedException.Create(targetType, source);
			}
			return patternString.Format();
		}
	}
}