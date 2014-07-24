using System.Xml;

namespace log4net.Repository
{
	public interface IXmlRepositoryConfigurator
	{
		void Configure(XmlElement element);
	}
}