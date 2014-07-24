namespace log4net.Repository.Hierarchy
{
	public interface ILoggerFactory
	{
		Logger CreateLogger(ILoggerRepository repository, string name);
	}
}