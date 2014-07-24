using log4net.Appender;

namespace log4net.Repository
{
	public interface IBasicRepositoryConfigurator
	{
		void Configure(IAppender appender);

		void Configure(params IAppender[] appenders);
	}
}