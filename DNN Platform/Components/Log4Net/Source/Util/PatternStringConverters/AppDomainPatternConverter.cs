using System.IO;

namespace log4net.Util.PatternStringConverters
{
	internal sealed class AppDomainPatternConverter : PatternConverter
	{
		public AppDomainPatternConverter()
		{
		}

		protected override void Convert(TextWriter writer, object state)
		{
			writer.Write(SystemInfo.ApplicationFriendlyName);
		}
	}
}