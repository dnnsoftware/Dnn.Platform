using log4net.Core;
using log4net.Util;

using System.IO;

namespace log4net.Layout.Pattern
{
	internal sealed class NdcPatternConverter : PatternLayoutConverter
	{
		public NdcPatternConverter()
		{
		}

		protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
		{
			PatternConverter.WriteObject(writer, loggingEvent.Repository, loggingEvent.LookupProperty("NDC"));
		}
	}
}