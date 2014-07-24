using log4net.Core;
using log4net.Util;

using System.IO;

namespace log4net.Layout.Pattern
{
	internal sealed class PropertyPatternConverter : PatternLayoutConverter
	{
		public PropertyPatternConverter()
		{
		}

		protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
		{
			if (this.Option == null)
			{
				PatternConverter.WriteDictionary(writer, loggingEvent.Repository, loggingEvent.GetProperties());
				return;
			}
			PatternConverter.WriteObject(writer, loggingEvent.Repository, loggingEvent.LookupProperty(this.Option));
		}
	}
}