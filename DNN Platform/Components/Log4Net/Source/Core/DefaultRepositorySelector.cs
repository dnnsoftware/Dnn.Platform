using log4net.Config;
using log4net.Plugin;
using log4net.Repository;
using log4net.Util;
using System;
using System.Collections;
using System.Reflection;

namespace log4net.Core
{
	public class DefaultRepositorySelector : IRepositorySelector
	{
		private const string DefaultRepositoryName = "log4net-default-repository";

		private readonly static Type declaringType;

		private readonly Hashtable m_name2repositoryMap = new Hashtable();

		private readonly Hashtable m_assembly2repositoryMap = new Hashtable();

		private readonly Hashtable m_alias2repositoryMap = new Hashtable();

		private readonly Type m_defaultRepositoryType;

		static DefaultRepositorySelector()
		{
			DefaultRepositorySelector.declaringType = typeof(DefaultRepositorySelector);
		}

		public DefaultRepositorySelector(Type defaultRepositoryType)
		{
			if (defaultRepositoryType == null)
			{
				throw new ArgumentNullException("defaultRepositoryType");
			}
			if (!typeof(ILoggerRepository).IsAssignableFrom(defaultRepositoryType))
			{
				throw SystemInfo.CreateArgumentOutOfRangeException("defaultRepositoryType", defaultRepositoryType, string.Concat("Parameter: defaultRepositoryType, Value: [", defaultRepositoryType, "] out of range. Argument must implement the ILoggerRepository interface"));
			}
			this.m_defaultRepositoryType = defaultRepositoryType;
			LogLog.Debug(DefaultRepositorySelector.declaringType, string.Concat("defaultRepositoryType [", this.m_defaultRepositoryType, "]"));
		}

		public void AliasRepository(string repositoryAlias, ILoggerRepository repositoryTarget)
		{
			if (repositoryAlias == null)
			{
				throw new ArgumentNullException("repositoryAlias");
			}
			if (repositoryTarget == null)
			{
				throw new ArgumentNullException("repositoryTarget");
			}
			lock (this)
			{
				if (this.m_alias2repositoryMap.Contains(repositoryAlias))
				{
					if (repositoryTarget != (ILoggerRepository)this.m_alias2repositoryMap[repositoryAlias])
					{
						string[] strArrays = new string[] { "Repository [", repositoryAlias, "] is already aliased to repository [", ((ILoggerRepository)this.m_alias2repositoryMap[repositoryAlias]).Name, "]. Aliases cannot be redefined." };
						throw new InvalidOperationException(string.Concat(strArrays));
					}
				}
				else if (!this.m_name2repositoryMap.Contains(repositoryAlias))
				{
					this.m_alias2repositoryMap[repositoryAlias] = repositoryTarget;
				}
				else if (repositoryTarget != (ILoggerRepository)this.m_name2repositoryMap[repositoryAlias])
				{
					string[] strArrays1 = new string[] { "Repository [", repositoryAlias, "] already exists and cannot be aliased to repository [", repositoryTarget.Name, "]." };
					throw new InvalidOperationException(string.Concat(strArrays1));
				}
			}
		}

		private void ConfigureRepository(Assembly assembly, ILoggerRepository repository)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}
			if (repository == null)
			{
				throw new ArgumentNullException("repository");
			}
			object[] customAttributes = Attribute.GetCustomAttributes(assembly, typeof(ConfiguratorAttribute), false);
			if (customAttributes != null && (int)customAttributes.Length > 0)
			{
				Array.Sort<object>(customAttributes);
				object[] objArray = customAttributes;
				for (int i = 0; i < (int)objArray.Length; i++)
				{
					ConfiguratorAttribute configuratorAttribute = (ConfiguratorAttribute)objArray[i];
					if (configuratorAttribute != null)
					{
						try
						{
							configuratorAttribute.Configure(assembly, repository);
						}
						catch (Exception exception1)
						{
							Exception exception = exception1;
							LogLog.Error(DefaultRepositorySelector.declaringType, string.Concat("Exception calling [", configuratorAttribute.GetType().FullName, "] .Configure method."), exception);
						}
					}
				}
			}
			if (repository.Name == "log4net-default-repository")
			{
				string appSetting = SystemInfo.GetAppSetting("log4net.Config");
				if (appSetting != null && appSetting.Length > 0)
				{
					string applicationBaseDirectory = null;
					try
					{
						applicationBaseDirectory = SystemInfo.ApplicationBaseDirectory;
					}
					catch (Exception exception3)
					{
						Exception exception2 = exception3;
						LogLog.Warn(DefaultRepositorySelector.declaringType, string.Concat("Exception getting ApplicationBaseDirectory. appSettings log4net.Config path [", appSetting, "] will be treated as an absolute URI"), exception2);
					}
					Uri uri = null;
					try
					{
						uri = (applicationBaseDirectory == null ? new Uri(appSetting) : new Uri(new Uri(applicationBaseDirectory), appSetting));
					}
					catch (Exception exception5)
					{
						Exception exception4 = exception5;
						LogLog.Error(DefaultRepositorySelector.declaringType, string.Concat("Exception while parsing log4net.Config file path [", appSetting, "]"), exception4);
					}
					if (uri != null)
					{
						LogLog.Debug(DefaultRepositorySelector.declaringType, string.Concat("Loading configuration for default repository from AppSettings specified Config URI [", uri.ToString(), "]"));
						try
						{
							XmlConfigurator.Configure(repository, uri);
						}
						catch (Exception exception7)
						{
							Exception exception6 = exception7;
							LogLog.Error(DefaultRepositorySelector.declaringType, string.Concat("Exception calling XmlConfigurator.Configure method with ConfigUri [", uri, "]"), exception6);
						}
					}
				}
			}
		}

		public ILoggerRepository CreateRepository(Assembly repositoryAssembly, Type repositoryType)
		{
			return this.CreateRepository(repositoryAssembly, repositoryType, "log4net-default-repository", true);
		}

		public ILoggerRepository CreateRepository(Assembly repositoryAssembly, Type repositoryType, string repositoryName, bool readAssemblyAttributes)
		{
			ILoggerRepository loggerRepository;
			if (repositoryAssembly == null)
			{
				throw new ArgumentNullException("repositoryAssembly");
			}
			if (repositoryType == null)
			{
				repositoryType = this.m_defaultRepositoryType;
			}
			lock (this)
			{
				ILoggerRepository item = this.m_assembly2repositoryMap[repositoryAssembly] as ILoggerRepository;
				if (item == null)
				{
					LogLog.Debug(DefaultRepositorySelector.declaringType, string.Concat("Creating repository for assembly [", repositoryAssembly, "]"));
					string str = repositoryName;
					Type type = repositoryType;
					if (readAssemblyAttributes)
					{
						this.GetInfoForAssembly(repositoryAssembly, ref str, ref type);
					}
					Type type1 = DefaultRepositorySelector.declaringType;
					object[] objArray = new object[] { "Assembly [", repositoryAssembly, "] using repository [", str, "] and repository type [", type, "]" };
					LogLog.Debug(type1, string.Concat(objArray));
					item = this.m_name2repositoryMap[str] as ILoggerRepository;
					if (item != null)
					{
						Type type2 = DefaultRepositorySelector.declaringType;
						string[] fullName = new string[] { "repository [", str, "] already exists, using repository type [", item.GetType().FullName, "]" };
						LogLog.Debug(type2, string.Concat(fullName));
						if (readAssemblyAttributes)
						{
							try
							{
								this.LoadPlugins(repositoryAssembly, item);
							}
							catch (Exception exception1)
							{
								Exception exception = exception1;
								LogLog.Error(DefaultRepositorySelector.declaringType, string.Concat("Failed to configure repository [", str, "] from assembly attributes."), exception);
							}
						}
					}
					else
					{
						item = this.CreateRepository(str, type);
						if (readAssemblyAttributes)
						{
							try
							{
								this.LoadAliases(repositoryAssembly, item);
								this.LoadPlugins(repositoryAssembly, item);
								this.ConfigureRepository(repositoryAssembly, item);
							}
							catch (Exception exception3)
							{
								Exception exception2 = exception3;
								LogLog.Error(DefaultRepositorySelector.declaringType, string.Concat("Failed to configure repository [", str, "] from assembly attributes."), exception2);
							}
						}
					}
					this.m_assembly2repositoryMap[repositoryAssembly] = item;
				}
				loggerRepository = item;
			}
			return loggerRepository;
		}

		public ILoggerRepository CreateRepository(string repositoryName, Type repositoryType)
		{
			ILoggerRepository loggerRepository;
			if (repositoryName == null)
			{
				throw new ArgumentNullException("repositoryName");
			}
			if (repositoryType == null)
			{
				repositoryType = this.m_defaultRepositoryType;
			}
			lock (this)
			{
				ILoggerRepository item = null;
				item = this.m_name2repositoryMap[repositoryName] as ILoggerRepository;
				if (item != null)
				{
					throw new LogException(string.Concat("Repository [", repositoryName, "] is already defined. Repositories cannot be redefined."));
				}
				ILoggerRepository item1 = this.m_alias2repositoryMap[repositoryName] as ILoggerRepository;
				if (item1 != null)
				{
					if (item1.GetType() != repositoryType)
					{
						Type type = DefaultRepositorySelector.declaringType;
						string[] strArrays = new string[] { "Failed to alias repository [", repositoryName, "] to existing repository [", item1.Name, "]. Requested repository type [", repositoryType.FullName, "] is not compatible with existing type [", item1.GetType().FullName, "]" };
						LogLog.Error(type, string.Concat(strArrays));
					}
					else
					{
						Type type1 = DefaultRepositorySelector.declaringType;
						string[] strArrays1 = new string[] { "Aliasing repository [", repositoryName, "] to existing repository [", item1.Name, "]" };
						LogLog.Debug(type1, string.Concat(strArrays1));
						item = item1;
						this.m_name2repositoryMap[repositoryName] = item;
					}
				}
				if (item == null)
				{
					Type type2 = DefaultRepositorySelector.declaringType;
					object[] objArray = new object[] { "Creating repository [", repositoryName, "] using type [", repositoryType, "]" };
					LogLog.Debug(type2, string.Concat(objArray));
					item = (ILoggerRepository)Activator.CreateInstance(repositoryType);
					item.Name = repositoryName;
					this.m_name2repositoryMap[repositoryName] = item;
					this.OnLoggerRepositoryCreatedEvent(item);
				}
				loggerRepository = item;
			}
			return loggerRepository;
		}

		public bool ExistsRepository(string repositoryName)
		{
			bool flag;
			lock (this)
			{
				flag = this.m_name2repositoryMap.ContainsKey(repositoryName);
			}
			return flag;
		}

		public ILoggerRepository[] GetAllRepositories()
		{
			ILoggerRepository[] loggerRepositoryArray;
			lock (this)
			{
				ICollection values = this.m_name2repositoryMap.Values;
				ILoggerRepository[] loggerRepositoryArray1 = new ILoggerRepository[values.Count];
				values.CopyTo(loggerRepositoryArray1, 0);
				loggerRepositoryArray = loggerRepositoryArray1;
			}
			return loggerRepositoryArray;
		}

		private void GetInfoForAssembly(Assembly assembly, ref string repositoryName, ref Type repositoryType)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}
			try
			{
				Type type = DefaultRepositorySelector.declaringType;
				string[] fullName = new string[] { "Assembly [", assembly.FullName, "] Loaded From [", SystemInfo.AssemblyLocationInfo(assembly), "]" };
				LogLog.Debug(type, string.Concat(fullName));
			}
			catch
			{
			}
			try
			{
				object[] customAttributes = Attribute.GetCustomAttributes(assembly, typeof(RepositoryAttribute), false);
				if (customAttributes == null || (int)customAttributes.Length == 0)
				{
					LogLog.Debug(DefaultRepositorySelector.declaringType, string.Concat("Assembly [", assembly, "] does not have a RepositoryAttribute specified."));
				}
				else
				{
					if ((int)customAttributes.Length > 1)
					{
						LogLog.Error(DefaultRepositorySelector.declaringType, string.Concat("Assembly [", assembly, "] has multiple log4net.Config.RepositoryAttribute assembly attributes. Only using first occurrence."));
					}
					RepositoryAttribute repositoryAttribute = customAttributes[0] as RepositoryAttribute;
					if (repositoryAttribute != null)
					{
						if (repositoryAttribute.Name != null)
						{
							repositoryName = repositoryAttribute.Name;
						}
						if (repositoryAttribute.RepositoryType != null)
						{
							if (!typeof(ILoggerRepository).IsAssignableFrom(repositoryAttribute.RepositoryType))
							{
								LogLog.Error(DefaultRepositorySelector.declaringType, string.Concat("DefaultRepositorySelector: Repository Type [", repositoryAttribute.RepositoryType, "] must implement the ILoggerRepository interface."));
							}
							else
							{
								repositoryType = repositoryAttribute.RepositoryType;
							}
						}
					}
					else
					{
						LogLog.Error(DefaultRepositorySelector.declaringType, string.Concat("Assembly [", assembly, "] has a RepositoryAttribute but it does not!."));
					}
				}
			}
			catch (Exception exception)
			{
				LogLog.Error(DefaultRepositorySelector.declaringType, "Unhandled exception in GetInfoForAssembly", exception);
			}
		}

		public ILoggerRepository GetRepository(Assembly repositoryAssembly)
		{
			if (repositoryAssembly == null)
			{
				throw new ArgumentNullException("repositoryAssembly");
			}
			return this.CreateRepository(repositoryAssembly, this.m_defaultRepositoryType);
		}

		public ILoggerRepository GetRepository(string repositoryName)
		{
			ILoggerRepository loggerRepository;
			if (repositoryName == null)
			{
				throw new ArgumentNullException("repositoryName");
			}
			lock (this)
			{
				ILoggerRepository item = this.m_name2repositoryMap[repositoryName] as ILoggerRepository;
				if (item == null)
				{
					throw new LogException(string.Concat("Repository [", repositoryName, "] is NOT defined."));
				}
				loggerRepository = item;
			}
			return loggerRepository;
		}

		private void LoadAliases(Assembly assembly, ILoggerRepository repository)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}
			if (repository == null)
			{
				throw new ArgumentNullException("repository");
			}
			object[] customAttributes = Attribute.GetCustomAttributes(assembly, typeof(AliasRepositoryAttribute), false);
			if (customAttributes != null && (int)customAttributes.Length > 0)
			{
				object[] objArray = customAttributes;
				for (int i = 0; i < (int)objArray.Length; i++)
				{
					AliasRepositoryAttribute aliasRepositoryAttribute = (AliasRepositoryAttribute)objArray[i];
					try
					{
						this.AliasRepository(aliasRepositoryAttribute.Name, repository);
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						LogLog.Error(DefaultRepositorySelector.declaringType, string.Concat("Failed to alias repository [", aliasRepositoryAttribute.Name, "]"), exception);
					}
				}
			}
		}

		private void LoadPlugins(Assembly assembly, ILoggerRepository repository)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}
			if (repository == null)
			{
				throw new ArgumentNullException("repository");
			}
			object[] customAttributes = Attribute.GetCustomAttributes(assembly, typeof(PluginAttribute), false);
			if (customAttributes != null && (int)customAttributes.Length > 0)
			{
				object[] objArray = customAttributes;
				for (int i = 0; i < (int)objArray.Length; i++)
				{
					IPluginFactory pluginFactory = (IPluginFactory)objArray[i];
					try
					{
						repository.PluginMap.Add(pluginFactory.CreatePlugin());
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						LogLog.Error(DefaultRepositorySelector.declaringType, string.Concat("Failed to create plugin. Attribute [", pluginFactory.ToString(), "]"), exception);
					}
				}
			}
		}

		protected virtual void OnLoggerRepositoryCreatedEvent(ILoggerRepository repository)
		{
			LoggerRepositoryCreationEventHandler loggerRepositoryCreationEventHandler = this.m_loggerRepositoryCreatedEvent;
			if (loggerRepositoryCreationEventHandler != null)
			{
				loggerRepositoryCreationEventHandler(this, new LoggerRepositoryCreationEventArgs(repository));
			}
		}

		public event LoggerRepositoryCreationEventHandler LoggerRepositoryCreatedEvent
		{
			add
			{
				this.m_loggerRepositoryCreatedEvent += value;
			}
			remove
			{
				this.m_loggerRepositoryCreatedEvent -= value;
			}
		}

		private event LoggerRepositoryCreationEventHandler m_loggerRepositoryCreatedEvent;
	}
}