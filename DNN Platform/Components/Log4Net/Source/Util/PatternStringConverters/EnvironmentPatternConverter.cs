using System;
using System.IO;
using System.Security;

namespace log4net.Util.PatternStringConverters
{
	internal sealed class EnvironmentPatternConverter : PatternConverter
	{
		private readonly static Type declaringType;

		static EnvironmentPatternConverter()
		{
			EnvironmentPatternConverter.declaringType = typeof(EnvironmentPatternConverter);
		}

		public EnvironmentPatternConverter()
		{
		}

		protected override void Convert(TextWriter writer, object state)
		{
			try
			{
				if (this.Option != null && this.Option.Length > 0)
				{
					string environmentVariable = Environment.GetEnvironmentVariable(this.Option) ?? Environment.GetEnvironmentVariable(this.Option, EnvironmentVariableTarget.User) ?? Environment.GetEnvironmentVariable(this.Option, EnvironmentVariableTarget.Machine);
					if (environmentVariable != null && environmentVariable.Length > 0)
					{
						writer.Write(environmentVariable);
					}
				}
			}
			catch (SecurityException securityException)
			{
				LogLog.Debug(EnvironmentPatternConverter.declaringType, "Security exception while trying to expand environment variables. Error Ignored. No Expansion.", securityException);
			}
			catch (Exception exception)
			{
				LogLog.Error(EnvironmentPatternConverter.declaringType, "Error occurred while converting environment variable.", exception);
			}
		}
	}
}