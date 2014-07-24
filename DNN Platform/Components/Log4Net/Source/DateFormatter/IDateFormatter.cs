using System;
using System.IO;

namespace log4net.DateFormatter
{
	public interface IDateFormatter
	{
		void FormatDate(DateTime dateToFormat, TextWriter writer);
	}
}