using log4net.Core;
using log4net.Util.TypeConverters;

namespace log4net.Layout
{
	[TypeConverter(typeof(RawLayoutConverter))]
	public interface IRawLayout
	{
		object Format(LoggingEvent loggingEvent);
	}
}