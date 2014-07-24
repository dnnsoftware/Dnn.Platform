using System;
using System.IO;
using System.Security;

namespace log4net.Util.PatternStringConverters
{
	internal sealed class EnvironmentFolderPathPatternConverter : PatternConverter
	{
		private readonly static Type declaringType;

		static EnvironmentFolderPathPatternConverter()
		{
			EnvironmentFolderPathPatternConverter.declaringType = typeof(EnvironmentFolderPathPatternConverter);
		}

		public EnvironmentFolderPathPatternConverter()
		{
		}

		protected override void Convert(TextWriter writer, object state)
		{
			try
			{
				if (this.Option != null && this.Option.Length > 0)
				{
					Environment.SpecialFolder specialFolder = (Environment.SpecialFolder)Enum.Parse(typeof(Environment.SpecialFolder), this.Option, true);
					string folderPath = Environment.GetFolderPath(specialFolder);
					if (folderPath != null && folderPath.Length > 0)
					{
						writer.Write(folderPath);
					}
				}
			}
			catch (SecurityException securityException)
			{
				LogLog.Debug(EnvironmentFolderPathPatternConverter.declaringType, "Security exception while trying to expand environment variables. Error Ignored. No Expansion.", securityException);
			}
			catch (Exception exception)
			{
				LogLog.Error(EnvironmentFolderPathPatternConverter.declaringType, "Error occurred while converting environment variable.", exception);
			}
		}
	}
}