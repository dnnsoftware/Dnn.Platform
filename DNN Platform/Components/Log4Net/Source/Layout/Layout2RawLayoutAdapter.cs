using log4net.Core;

using System.Globalization;
using System.IO;

namespace log4net.Layout
{
	public class Layout2RawLayoutAdapter : IRawLayout
	{
		private ILayout m_layout;

		public Layout2RawLayoutAdapter(ILayout layout)
		{
			this.m_layout = layout;
		}

		public virtual object Format(LoggingEvent loggingEvent)
		{
			StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
			this.m_layout.Format(stringWriter, loggingEvent);
			return stringWriter.ToString();
		}
	}
}