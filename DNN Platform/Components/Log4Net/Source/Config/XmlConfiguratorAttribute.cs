using log4net.Repository;
using log4net.Util;
using System;
using System.Collections;
using System.IO;
using System.Reflection;

namespace log4net.Config
{
	[AttributeUsage(AttributeTargets.Assembly)]
	[Serializable]
	public class XmlConfiguratorAttribute : ConfiguratorAttribute
	{
		private string m_configFile;

		private string m_configFileExtension;

		private bool m_configureAndWatch;

		private readonly static Type declaringType;

		public string ConfigFile
		{
			get
			{
				return this.m_configFile;
			}
			set
			{
				this.m_configFile = value;
			}
		}

		public string ConfigFileExtension
		{
			get
			{
				return this.m_configFileExtension;
			}
			set
			{
				this.m_configFileExtension = value;
			}
		}

		public bool Watch
		{
			get
			{
				return this.m_configureAndWatch;
			}
			set
			{
				this.m_configureAndWatch = value;
			}
		}

		static XmlConfiguratorAttribute()
		{
			XmlConfiguratorAttribute.declaringType = typeof(XmlConfiguratorAttribute);
		}

		public XmlConfiguratorAttribute() : base(0)
		{
		}

		public override void Configure(Assembly sourceAssembly, ILoggerRepository targetRepository)
		{
			IList arrayLists = new ArrayList();
			using (LogLog.LogReceivedAdapter logReceivedAdapter = new LogLog.LogReceivedAdapter(arrayLists))
			{
				string applicationBaseDirectory = null;
				try
				{
					applicationBaseDirectory = SystemInfo.ApplicationBaseDirectory;
				}
				catch
				{
				}
				if (applicationBaseDirectory == null || (new Uri(applicationBaseDirectory)).IsFile)
				{
					this.ConfigureFromFile(sourceAssembly, targetRepository);
				}
				else
				{
					this.ConfigureFromUri(sourceAssembly, targetRepository);
				}
			}
			targetRepository.ConfigurationMessages = arrayLists;
		}

		private void ConfigureFromFile(Assembly sourceAssembly, ILoggerRepository targetRepository)
		{
			string configurationFileLocation = null;
			if (this.m_configFile != null && this.m_configFile.Length != 0)
			{
				string applicationBaseDirectory = null;
				try
				{
					applicationBaseDirectory = SystemInfo.ApplicationBaseDirectory;
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					LogLog.Warn(XmlConfiguratorAttribute.declaringType, string.Concat("Exception getting ApplicationBaseDirectory. ConfigFile property path [", this.m_configFile, "] will be treated as an absolute path."), exception);
				}
				configurationFileLocation = (applicationBaseDirectory == null ? this.m_configFile : Path.Combine(applicationBaseDirectory, this.m_configFile));
			}
			else if (this.m_configFileExtension == null || this.m_configFileExtension.Length == 0)
			{
				try
				{
					configurationFileLocation = SystemInfo.ConfigurationFileLocation;
				}
				catch (Exception exception2)
				{
					LogLog.Error(XmlConfiguratorAttribute.declaringType, "XmlConfiguratorAttribute: Exception getting ConfigurationFileLocation. Must be able to resolve ConfigurationFileLocation when ConfigFile and ConfigFileExtension properties are not set.", exception2);
				}
			}
			else
			{
				if (this.m_configFileExtension[0] != '.')
				{
					this.m_configFileExtension = string.Concat(".", this.m_configFileExtension);
				}
				string str = null;
				try
				{
					str = SystemInfo.ApplicationBaseDirectory;
				}
				catch (Exception exception3)
				{
					LogLog.Error(XmlConfiguratorAttribute.declaringType, "Exception getting ApplicationBaseDirectory. Must be able to resolve ApplicationBaseDirectory and AssemblyFileName when ConfigFileExtension property is set.", exception3);
				}
				if (str != null)
				{
					configurationFileLocation = Path.Combine(str, string.Concat(SystemInfo.AssemblyFileName(sourceAssembly), this.m_configFileExtension));
				}
			}
			if (configurationFileLocation != null)
			{
				this.ConfigureFromFile(targetRepository, new FileInfo(configurationFileLocation));
			}
		}

		private void ConfigureFromFile(ILoggerRepository targetRepository, FileInfo configFile)
		{
			if (this.m_configureAndWatch)
			{
				XmlConfigurator.ConfigureAndWatch(targetRepository, configFile);
				return;
			}
			XmlConfigurator.Configure(targetRepository, configFile);
		}

		private void ConfigureFromUri(Assembly sourceAssembly, ILoggerRepository targetRepository)
		{
			Uri uri = null;
			if (this.m_configFile != null && this.m_configFile.Length != 0)
			{
				string applicationBaseDirectory = null;
				try
				{
					applicationBaseDirectory = SystemInfo.ApplicationBaseDirectory;
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					LogLog.Warn(XmlConfiguratorAttribute.declaringType, string.Concat("Exception getting ApplicationBaseDirectory. ConfigFile property path [", this.m_configFile, "] will be treated as an absolute URI."), exception);
				}
				uri = (applicationBaseDirectory == null ? new Uri(this.m_configFile) : new Uri(new Uri(applicationBaseDirectory), this.m_configFile));
			}
			else if (this.m_configFileExtension == null || this.m_configFileExtension.Length == 0)
			{
				string configurationFileLocation = null;
				try
				{
					configurationFileLocation = SystemInfo.ConfigurationFileLocation;
				}
				catch (Exception exception2)
				{
					LogLog.Error(XmlConfiguratorAttribute.declaringType, "XmlConfiguratorAttribute: Exception getting ConfigurationFileLocation. Must be able to resolve ConfigurationFileLocation when ConfigFile and ConfigFileExtension properties are not set.", exception2);
				}
				if (configurationFileLocation != null)
				{
					uri = new Uri(configurationFileLocation);
				}
			}
			else
			{
				if (this.m_configFileExtension[0] != '.')
				{
					this.m_configFileExtension = string.Concat(".", this.m_configFileExtension);
				}
				string str = null;
				try
				{
					str = SystemInfo.ConfigurationFileLocation;
				}
				catch (Exception exception3)
				{
					LogLog.Error(XmlConfiguratorAttribute.declaringType, "XmlConfiguratorAttribute: Exception getting ConfigurationFileLocation. Must be able to resolve ConfigurationFileLocation when the ConfigFile property are not set.", exception3);
				}
				if (str != null)
				{
					UriBuilder uriBuilder = new UriBuilder(new Uri(str));
					string path = uriBuilder.Path;
					int num = path.LastIndexOf(".");
					if (num >= 0)
					{
						path = path.Substring(0, num);
					}
					path = string.Concat(path, this.m_configFileExtension);
					uriBuilder.Path = path;
					uri = uriBuilder.Uri;
				}
			}
			if (uri != null)
			{
				if (uri.IsFile)
				{
					this.ConfigureFromFile(targetRepository, new FileInfo(uri.LocalPath));
					return;
				}
				if (this.m_configureAndWatch)
				{
					LogLog.Warn(XmlConfiguratorAttribute.declaringType, "XmlConfiguratorAttribute: Unable to watch config file loaded from a URI");
				}
				XmlConfigurator.Configure(targetRepository, uri);
			}
		}
	}
}