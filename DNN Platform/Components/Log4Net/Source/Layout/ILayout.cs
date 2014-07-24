using log4net.Core;

using System.IO;

namespace log4net.Layout
{
	public interface ILayout
	{
		string ContentType
		{
			get;
		}

		string Footer
		{
			get;
		}

		string Header
		{
			get;
		}

		bool IgnoresException
		{
			get;
		}

		void Format(TextWriter writer, LoggingEvent loggingEvent);
	}
}