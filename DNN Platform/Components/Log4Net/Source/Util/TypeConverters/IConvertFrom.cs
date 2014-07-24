using System;

namespace log4net.Util.TypeConverters
{
	public interface IConvertFrom
	{
		bool CanConvertFrom(Type sourceType);

		object ConvertFrom(object source);
	}
}