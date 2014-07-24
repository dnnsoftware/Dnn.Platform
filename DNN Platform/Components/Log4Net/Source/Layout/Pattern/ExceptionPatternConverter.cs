using log4net.Core;
using log4net.Util;

using System.IO;

namespace log4net.Layout.Pattern
{
	internal sealed class ExceptionPatternConverter : PatternLayoutConverter
	{
		public ExceptionPatternConverter()
		{
			this.IgnoresException = false;
		}

		protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
		{
			if (loggingEvent.ExceptionObject == null || this.Option == null || this.Option.Length <= 0)
			{
				string exceptionString = loggingEvent.GetExceptionString();
				if (exceptionString != null && exceptionString.Length > 0)
				{
					writer.WriteLine(exceptionString);
				}
			}
			else
			{
				string lower = this.Option.ToLower();
				string str = lower;
				if (lower != null)
				{
					if (str == "message")
					{
						PatternConverter.WriteObject(writer, loggingEvent.Repository, loggingEvent.ExceptionObject.Message);
						return;
					}
					if (str == "source")
					{
						PatternConverter.WriteObject(writer, loggingEvent.Repository, loggingEvent.ExceptionObject.Source);
						return;
					}
					if (str == "stacktrace")
					{
						PatternConverter.WriteObject(writer, loggingEvent.Repository, loggingEvent.ExceptionObject.StackTrace);
						return;
					}
					if (str == "targetsite")
					{
						PatternConverter.WriteObject(writer, loggingEvent.Repository, loggingEvent.ExceptionObject.TargetSite);
						return;
					}
					if (str != "helplink")
					{
						return;
					}
					PatternConverter.WriteObject(writer, loggingEvent.Repository, loggingEvent.ExceptionObject.HelpLink);
					return;
				}
			}
		}
	}
}