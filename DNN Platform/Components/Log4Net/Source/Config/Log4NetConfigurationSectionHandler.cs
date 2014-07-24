using System.Configuration;
using System.Xml;

namespace log4net.Config
{
	public class Log4NetConfigurationSectionHandler : IConfigurationSectionHandler
	{
		public Log4NetConfigurationSectionHandler()
		{
		}

		public object Create(object parent, object configContext, XmlNode section)
		{
			return section;
		}
	}
}