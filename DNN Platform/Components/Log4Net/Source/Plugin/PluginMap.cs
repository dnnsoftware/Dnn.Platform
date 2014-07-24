using log4net.Repository;
using System;
using System.Collections;

namespace log4net.Plugin
{
	public sealed class PluginMap
	{
		private readonly Hashtable m_mapName2Plugin = new Hashtable();

		private readonly ILoggerRepository m_repository;

		public PluginCollection AllPlugins
		{
			get
			{
				PluginCollection pluginCollections;
				lock (this)
				{
					pluginCollections = new PluginCollection(this.m_mapName2Plugin.Values);
				}
				return pluginCollections;
			}
		}

		public IPlugin this[string name]
		{
			get
			{
				IPlugin item;
				if (name == null)
				{
					throw new ArgumentNullException("name");
				}
				lock (this)
				{
					item = (IPlugin)this.m_mapName2Plugin[name];
				}
				return item;
			}
		}

		public PluginMap(ILoggerRepository repository)
		{
			this.m_repository = repository;
		}

		public void Add(IPlugin plugin)
		{
			if (plugin == null)
			{
				throw new ArgumentNullException("plugin");
			}
			IPlugin item = null;
			lock (this)
			{
				item = this.m_mapName2Plugin[plugin.Name] as IPlugin;
				this.m_mapName2Plugin[plugin.Name] = plugin;
			}
			if (item != null)
			{
				item.Shutdown();
			}
			plugin.Attach(this.m_repository);
		}

		public void Remove(IPlugin plugin)
		{
			if (plugin == null)
			{
				throw new ArgumentNullException("plugin");
			}
			lock (this)
			{
				this.m_mapName2Plugin.Remove(plugin.Name);
			}
		}
	}
}