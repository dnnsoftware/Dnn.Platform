using log4net.Repository;
using log4net.Util;
using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Xml;

namespace log4net.Config
{
	public sealed class XmlConfigurator
	{
		private readonly static Hashtable m_repositoryName2ConfigAndWatchHandler;

		private readonly static Type declaringType;

		static XmlConfigurator()
		{
			XmlConfigurator.m_repositoryName2ConfigAndWatchHandler = new Hashtable();
			XmlConfigurator.declaringType = typeof(XmlConfigurator);
		}

		private XmlConfigurator()
		{
		}

		public static ICollection Configure()
		{
			return XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetCallingAssembly()));
		}

		public static ICollection Configure(ILoggerRepository repository)
		{
			ArrayList arrayLists = new ArrayList();
			using (LogLog.LogReceivedAdapter logReceivedAdapter = new LogLog.LogReceivedAdapter(arrayLists))
			{
				XmlConfigurator.InternalConfigure(repository);
			}
			repository.ConfigurationMessages = arrayLists;
			return arrayLists;
		}

		public static ICollection Configure(XmlElement element)
		{
			ArrayList arrayLists = new ArrayList();
			ILoggerRepository repository = LogManager.GetRepository(Assembly.GetCallingAssembly());
			using (LogLog.LogReceivedAdapter logReceivedAdapter = new LogLog.LogReceivedAdapter(arrayLists))
			{
				XmlConfigurator.InternalConfigureFromXml(repository, element);
			}
			repository.ConfigurationMessages = arrayLists;
			return arrayLists;
		}

		public static ICollection Configure(ILoggerRepository repository, XmlElement element)
		{
			ArrayList arrayLists = new ArrayList();
			using (LogLog.LogReceivedAdapter logReceivedAdapter = new LogLog.LogReceivedAdapter(arrayLists))
			{
				LogLog.Debug(XmlConfigurator.declaringType, string.Concat("configuring repository [", repository.Name, "] using XML element"));
				XmlConfigurator.InternalConfigureFromXml(repository, element);
			}
			repository.ConfigurationMessages = arrayLists;
			return arrayLists;
		}

		public static ICollection Configure(FileInfo configFile)
		{
			ArrayList arrayLists = new ArrayList();
			using (LogLog.LogReceivedAdapter logReceivedAdapter = new LogLog.LogReceivedAdapter(arrayLists))
			{
				XmlConfigurator.InternalConfigure(LogManager.GetRepository(Assembly.GetCallingAssembly()), configFile);
			}
			return arrayLists;
		}

		public static ICollection Configure(Uri configUri)
		{
			ArrayList arrayLists = new ArrayList();
			ILoggerRepository repository = LogManager.GetRepository(Assembly.GetCallingAssembly());
			using (LogLog.LogReceivedAdapter logReceivedAdapter = new LogLog.LogReceivedAdapter(arrayLists))
			{
				XmlConfigurator.InternalConfigure(repository, configUri);
			}
			repository.ConfigurationMessages = arrayLists;
			return arrayLists;
		}

		public static ICollection Configure(Stream configStream)
		{
			ArrayList arrayLists = new ArrayList();
			ILoggerRepository repository = LogManager.GetRepository(Assembly.GetCallingAssembly());
			using (LogLog.LogReceivedAdapter logReceivedAdapter = new LogLog.LogReceivedAdapter(arrayLists))
			{
				XmlConfigurator.InternalConfigure(repository, configStream);
			}
			repository.ConfigurationMessages = arrayLists;
			return arrayLists;
		}

		public static ICollection Configure(ILoggerRepository repository, FileInfo configFile)
		{
			ArrayList arrayLists = new ArrayList();
			using (LogLog.LogReceivedAdapter logReceivedAdapter = new LogLog.LogReceivedAdapter(arrayLists))
			{
				XmlConfigurator.InternalConfigure(repository, configFile);
			}
			repository.ConfigurationMessages = arrayLists;
			return arrayLists;
		}

		public static ICollection Configure(ILoggerRepository repository, Uri configUri)
		{
			ArrayList arrayLists = new ArrayList();
			using (LogLog.LogReceivedAdapter logReceivedAdapter = new LogLog.LogReceivedAdapter(arrayLists))
			{
				XmlConfigurator.InternalConfigure(repository, configUri);
			}
			repository.ConfigurationMessages = arrayLists;
			return arrayLists;
		}

		public static ICollection Configure(ILoggerRepository repository, Stream configStream)
		{
			ArrayList arrayLists = new ArrayList();
			using (LogLog.LogReceivedAdapter logReceivedAdapter = new LogLog.LogReceivedAdapter(arrayLists))
			{
				XmlConfigurator.InternalConfigure(repository, configStream);
			}
			repository.ConfigurationMessages = arrayLists;
			return arrayLists;
		}

		public static ICollection ConfigureAndWatch(FileInfo configFile)
		{
			ArrayList arrayLists = new ArrayList();
			ILoggerRepository repository = LogManager.GetRepository(Assembly.GetCallingAssembly());
			using (LogLog.LogReceivedAdapter logReceivedAdapter = new LogLog.LogReceivedAdapter(arrayLists))
			{
				XmlConfigurator.InternalConfigureAndWatch(repository, configFile);
			}
			repository.ConfigurationMessages = arrayLists;
			return arrayLists;
		}

		public static ICollection ConfigureAndWatch(ILoggerRepository repository, FileInfo configFile)
		{
			ArrayList arrayLists = new ArrayList();
			using (LogLog.LogReceivedAdapter logReceivedAdapter = new LogLog.LogReceivedAdapter(arrayLists))
			{
				XmlConfigurator.InternalConfigureAndWatch(repository, configFile);
			}
			repository.ConfigurationMessages = arrayLists;
			return arrayLists;
		}

		private static void InternalConfigure(ILoggerRepository repository)
		{
			LogLog.Debug(XmlConfigurator.declaringType, string.Concat("configuring repository [", repository.Name, "] using .config file section"));
			try
			{
				LogLog.Debug(XmlConfigurator.declaringType, string.Concat("Application config file is [", SystemInfo.ConfigurationFileLocation, "]"));
			}
			catch
			{
				LogLog.Debug(XmlConfigurator.declaringType, "Application config file location unknown");
			}
			try
			{
				XmlElement section = null;
				section = ConfigurationManager.GetSection("log4net") as XmlElement;
				if (section != null)
				{
					XmlConfigurator.InternalConfigureFromXml(repository, section);
				}
				else
				{
					LogLog.Error(XmlConfigurator.declaringType, "Failed to find configuration section 'log4net' in the application's .config file. Check your .config file for the <log4net> and <configSections> elements. The configuration section should look like: <section name=\"log4net\" type=\"log4net.Config.Log4NetConfigurationSectionHandler,log4net\" />");
				}
			}
			catch (ConfigurationException configurationException1)
			{
				ConfigurationException configurationException = configurationException1;
				if (configurationException.BareMessage.IndexOf("Unrecognized element") < 0)
				{
					string str = string.Concat("<section name=\"log4net\" type=\"log4net.Config.Log4NetConfigurationSectionHandler,", Assembly.GetExecutingAssembly().FullName, "\" />");
					LogLog.Error(XmlConfigurator.declaringType, string.Concat("Failed to parse config file. Is the <configSections> specified as: ", str), configurationException);
				}
				else
				{
					LogLog.Error(XmlConfigurator.declaringType, "Failed to parse config file. Check your .config file is well formed XML.", configurationException);
				}
			}
		}

		private static void InternalConfigure(ILoggerRepository repository, FileInfo configFile)
		{
			Type type = XmlConfigurator.declaringType;
			object[] name = new object[] { "configuring repository [", repository.Name, "] using file [", configFile, "]" };
			LogLog.Debug(type, string.Concat(name));
			if (configFile == null)
			{
				LogLog.Error(XmlConfigurator.declaringType, "Configure called with null 'configFile' parameter");
				return;
			}
			if (!File.Exists(configFile.FullName))
			{
				LogLog.Debug(XmlConfigurator.declaringType, string.Concat("config file [", configFile.FullName, "] not found. Configuration unchanged."));
			}
			else
			{
				FileStream fileStream = null;
				int num = 5;
				while (true)
				{
					int num1 = num - 1;
					num = num1;
					if (num1 < 0)
					{
						break;
					}
					try
					{
						fileStream = configFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
						break;
					}
					catch (IOException oException1)
					{
						IOException oException = oException1;
						if (num == 0)
						{
							LogLog.Error(XmlConfigurator.declaringType, string.Concat("Failed to open XML config file [", configFile.Name, "]"), oException);
							fileStream = null;
						}
						Thread.Sleep(250);
					}
				}
				if (fileStream != null)
				{
					try
					{
						XmlConfigurator.InternalConfigure(repository, fileStream);
					}
					finally
					{
						fileStream.Close();
					}
				}
			}
		}

		private static void InternalConfigure(ILoggerRepository repository, Uri configUri)
		{
			Type type = XmlConfigurator.declaringType;
			object[] name = new object[] { "configuring repository [", repository.Name, "] using URI [", configUri, "]" };
			LogLog.Debug(type, string.Concat(name));
			if (configUri == null)
			{
				LogLog.Error(XmlConfigurator.declaringType, "Configure called with null 'configUri' parameter");
				return;
			}
			if (configUri.IsFile)
			{
				XmlConfigurator.InternalConfigure(repository, new FileInfo(configUri.LocalPath));
				return;
			}
			WebRequest defaultCredentials = null;
			try
			{
				defaultCredentials = WebRequest.Create(configUri);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				LogLog.Error(XmlConfigurator.declaringType, string.Concat("Failed to create WebRequest for URI [", configUri, "]"), exception);
			}
			if (defaultCredentials != null)
			{
				try
				{
					defaultCredentials.Credentials = CredentialCache.DefaultCredentials;
				}
				catch
				{
				}
				try
				{
					WebResponse response = defaultCredentials.GetResponse();
					if (response != null)
					{
						try
						{
							using (Stream responseStream = response.GetResponseStream())
							{
								XmlConfigurator.InternalConfigure(repository, responseStream);
							}
						}
						finally
						{
							response.Close();
						}
					}
				}
				catch (Exception exception3)
				{
					Exception exception2 = exception3;
					LogLog.Error(XmlConfigurator.declaringType, string.Concat("Failed to request config from URI [", configUri, "]"), exception2);
				}
			}
		}

		private static void InternalConfigure(ILoggerRepository repository, Stream configStream)
		{
			LogLog.Debug(XmlConfigurator.declaringType, string.Concat("configuring repository [", repository.Name, "] using stream"));
			if (configStream == null)
			{
				LogLog.Error(XmlConfigurator.declaringType, "Configure called with null 'configStream' parameter");
				return;
			}
			XmlDocument xmlDocument = new XmlDocument();
			try
			{
				XmlReaderSettings xmlReaderSetting = new XmlReaderSettings()
				{
					ProhibitDtd = false
				};
				xmlDocument.Load(XmlReader.Create(configStream, xmlReaderSetting));
			}
			catch (Exception exception)
			{
				LogLog.Error(XmlConfigurator.declaringType, "Error while loading XML configuration", exception);
				xmlDocument = null;
			}
			if (xmlDocument != null)
			{
				LogLog.Debug(XmlConfigurator.declaringType, "loading XML configuration");
				XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("log4net");
				if (elementsByTagName.Count == 0)
				{
					LogLog.Debug(XmlConfigurator.declaringType, "XML configuration does not contain a <log4net> element. Configuration Aborted.");
					return;
				}
				if (elementsByTagName.Count > 1)
				{
					LogLog.Error(XmlConfigurator.declaringType, string.Concat("XML configuration contains [", elementsByTagName.Count, "] <log4net> elements. Only one is allowed. Configuration Aborted."));
					return;
				}
				XmlConfigurator.InternalConfigureFromXml(repository, elementsByTagName[0] as XmlElement);
			}
		}

		private static void InternalConfigureAndWatch(ILoggerRepository repository, FileInfo configFile)
		{
			Type type = XmlConfigurator.declaringType;
			object[] name = new object[] { "configuring repository [", repository.Name, "] using file [", configFile, "] watching for file updates" };
			LogLog.Debug(type, string.Concat(name));
			if (configFile == null)
			{
				LogLog.Error(XmlConfigurator.declaringType, "ConfigureAndWatch called with null 'configFile' parameter");
				return;
			}
			XmlConfigurator.InternalConfigure(repository, configFile);
			try
			{
				lock (XmlConfigurator.m_repositoryName2ConfigAndWatchHandler)
				{
					XmlConfigurator.ConfigureAndWatchHandler item = (XmlConfigurator.ConfigureAndWatchHandler)XmlConfigurator.m_repositoryName2ConfigAndWatchHandler[repository.Name];
					if (item != null)
					{
						XmlConfigurator.m_repositoryName2ConfigAndWatchHandler.Remove(repository.Name);
						item.Dispose();
					}
					item = new XmlConfigurator.ConfigureAndWatchHandler(repository, configFile);
					XmlConfigurator.m_repositoryName2ConfigAndWatchHandler[repository.Name] = item;
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				LogLog.Error(XmlConfigurator.declaringType, string.Concat("Failed to initialize configuration file watcher for file [", configFile.FullName, "]"), exception);
			}
		}

		private static void InternalConfigureFromXml(ILoggerRepository repository, XmlElement element)
		{
			if (element == null)
			{
				LogLog.Error(XmlConfigurator.declaringType, "ConfigureFromXml called with null 'element' parameter");
				return;
			}
			if (repository == null)
			{
				LogLog.Error(XmlConfigurator.declaringType, "ConfigureFromXml called with null 'repository' parameter");
				return;
			}
			LogLog.Debug(XmlConfigurator.declaringType, string.Concat("Configuring Repository [", repository.Name, "]"));
			IXmlRepositoryConfigurator xmlRepositoryConfigurator = repository as IXmlRepositoryConfigurator;
			if (xmlRepositoryConfigurator == null)
			{
				LogLog.Warn(XmlConfigurator.declaringType, string.Concat("Repository [", repository, "] does not support the XmlConfigurator"));
				return;
			}
			XmlDocument xmlDocument = new XmlDocument();
			XmlElement xmlElement = (XmlElement)xmlDocument.AppendChild(xmlDocument.ImportNode(element, true));
			xmlRepositoryConfigurator.Configure(xmlElement);
		}

		private sealed class ConfigureAndWatchHandler : IDisposable
		{
			private const int TimeoutMillis = 500;

			private FileInfo m_configFile;

			private ILoggerRepository m_repository;

			private Timer m_timer;

			private FileSystemWatcher m_watcher;

			public ConfigureAndWatchHandler(ILoggerRepository repository, FileInfo configFile)
			{
				this.m_repository = repository;
				this.m_configFile = configFile;
				this.m_watcher = new FileSystemWatcher()
				{
					Path = this.m_configFile.DirectoryName,
					Filter = this.m_configFile.Name,
					NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime
				};
				this.m_watcher.Changed += new FileSystemEventHandler(this.ConfigureAndWatchHandler_OnChanged);
				this.m_watcher.Created += new FileSystemEventHandler(this.ConfigureAndWatchHandler_OnChanged);
				this.m_watcher.Deleted += new FileSystemEventHandler(this.ConfigureAndWatchHandler_OnChanged);
				this.m_watcher.Renamed += new RenamedEventHandler(this.ConfigureAndWatchHandler_OnRenamed);
				this.m_watcher.EnableRaisingEvents = true;
				this.m_timer = new Timer(new TimerCallback(this.OnWatchedFileChange), null, -1, -1);
			}

			private void ConfigureAndWatchHandler_OnChanged(object source, FileSystemEventArgs e)
			{
				Type type = XmlConfigurator.declaringType;
				object[] changeType = new object[] { "ConfigureAndWatchHandler: ", e.ChangeType, " [", this.m_configFile.FullName, "]" };
				LogLog.Debug(type, string.Concat(changeType));
				this.m_timer.Change(500, -1);
			}

			private void ConfigureAndWatchHandler_OnRenamed(object source, RenamedEventArgs e)
			{
				Type type = XmlConfigurator.declaringType;
				object[] changeType = new object[] { "ConfigureAndWatchHandler: ", e.ChangeType, " [", this.m_configFile.FullName, "]" };
				LogLog.Debug(type, string.Concat(changeType));
				this.m_timer.Change(500, -1);
			}

			public void Dispose()
			{
				this.m_watcher.EnableRaisingEvents = false;
				this.m_watcher.Dispose();
				this.m_timer.Dispose();
			}

			private void OnWatchedFileChange(object state)
			{
				XmlConfigurator.InternalConfigure(this.m_repository, this.m_configFile);
			}
		}
	}
}