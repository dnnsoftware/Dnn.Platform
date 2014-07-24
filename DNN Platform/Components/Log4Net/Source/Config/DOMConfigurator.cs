using log4net.Repository;
using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace log4net.Config
{
	[Obsolete("Use XmlConfigurator instead of DOMConfigurator")]
	public sealed class DOMConfigurator
	{
		private DOMConfigurator()
		{
		}

		[Obsolete("Use XmlConfigurator.Configure instead of DOMConfigurator.Configure")]
		public static void Configure()
		{
			XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetCallingAssembly()));
		}

		[Obsolete("Use XmlConfigurator.Configure instead of DOMConfigurator.Configure")]
		public static void Configure(ILoggerRepository repository)
		{
			XmlConfigurator.Configure(repository);
		}

		[Obsolete("Use XmlConfigurator.Configure instead of DOMConfigurator.Configure")]
		public static void Configure(XmlElement element)
		{
			XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetCallingAssembly()), element);
		}

		[Obsolete("Use XmlConfigurator.Configure instead of DOMConfigurator.Configure")]
		public static void Configure(ILoggerRepository repository, XmlElement element)
		{
			XmlConfigurator.Configure(repository, element);
		}

		[Obsolete("Use XmlConfigurator.Configure instead of DOMConfigurator.Configure")]
		public static void Configure(FileInfo configFile)
		{
			XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetCallingAssembly()), configFile);
		}

		[Obsolete("Use XmlConfigurator.Configure instead of DOMConfigurator.Configure")]
		public static void Configure(Stream configStream)
		{
			XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetCallingAssembly()), configStream);
		}

		[Obsolete("Use XmlConfigurator.Configure instead of DOMConfigurator.Configure")]
		public static void Configure(ILoggerRepository repository, FileInfo configFile)
		{
			XmlConfigurator.Configure(repository, configFile);
		}

		[Obsolete("Use XmlConfigurator.Configure instead of DOMConfigurator.Configure")]
		public static void Configure(ILoggerRepository repository, Stream configStream)
		{
			XmlConfigurator.Configure(repository, configStream);
		}

		[Obsolete("Use XmlConfigurator.ConfigureAndWatch instead of DOMConfigurator.ConfigureAndWatch")]
		public static void ConfigureAndWatch(FileInfo configFile)
		{
			XmlConfigurator.ConfigureAndWatch(LogManager.GetRepository(Assembly.GetCallingAssembly()), configFile);
		}

		[Obsolete("Use XmlConfigurator.ConfigureAndWatch instead of DOMConfigurator.ConfigureAndWatch")]
		public static void ConfigureAndWatch(ILoggerRepository repository, FileInfo configFile)
		{
			XmlConfigurator.ConfigureAndWatch(repository, configFile);
		}
	}
}