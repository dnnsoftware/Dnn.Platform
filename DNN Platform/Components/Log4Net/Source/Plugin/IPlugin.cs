using log4net.Repository;

namespace log4net.Plugin
{
	public interface IPlugin
	{
		string Name
		{
			get;
		}

		void Attach(ILoggerRepository repository);

		void Shutdown();
	}
}