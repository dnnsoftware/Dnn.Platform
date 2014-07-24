using log4net.Appender;

namespace log4net.Core
{
	public interface IAppenderAttachable
	{
		AppenderCollection Appenders
		{
			get;
		}

		void AddAppender(IAppender appender);

		IAppender GetAppender(string name);

		void RemoveAllAppenders();

		IAppender RemoveAppender(IAppender appender);

		IAppender RemoveAppender(string name);
	}
}