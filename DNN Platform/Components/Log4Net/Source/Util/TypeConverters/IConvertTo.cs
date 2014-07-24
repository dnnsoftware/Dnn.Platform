using System;

namespace log4net.Util.TypeConverters
{
	public interface IConvertTo
	{
		bool CanConvertTo(Type targetType);

		object ConvertTo(object source, Type targetType);
	}
}