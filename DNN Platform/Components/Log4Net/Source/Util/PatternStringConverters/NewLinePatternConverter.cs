using log4net.Core;

using System.Globalization;

namespace log4net.Util.PatternStringConverters
{
	internal sealed class NewLinePatternConverter : LiteralPatternConverter, IOptionHandler
	{
		public NewLinePatternConverter()
		{
		}

		public void ActivateOptions()
		{
			if (string.Compare(this.Option, "DOS", true, CultureInfo.InvariantCulture) == 0)
			{
				this.Option = "\r\n";
				return;
			}
			if (string.Compare(this.Option, "UNIX", true, CultureInfo.InvariantCulture) == 0)
			{
				this.Option = "\n";
				return;
			}
			this.Option = SystemInfo.NewLine;
		}
	}
}