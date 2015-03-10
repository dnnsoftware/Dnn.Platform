#region Apache License
//
// Licensed to the Apache Software Foundation (ASF) under one or more 
// contributor license agreements. See the NOTICE file distributed with
// this work for additional information regarding copyright ownership. 
// The ASF licenses this file to you under the Apache License, Version 2.0
// (the "License"); you may not use this file except in compliance with 
// the License. You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Collections;

using log4net.Util;
using log4net.Repository;

namespace log4net.Plugin
{
	/// <summary>
	/// Map of repository plugins.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This class is a name keyed map of the plugins that are
	/// attached to a repository.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public sealed class PluginMap
	{
		#region Public Instance Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="repository">The repository that the plugins should be attached to.</param>
		/// <remarks>
		/// <para>
		/// Initialize a new instance of the <see cref="PluginMap" /> class with a 
		/// repository that the plugins should be attached to.
		/// </para>
		/// </remarks>
		public PluginMap(ILoggerRepository repository)
		{
			m_repository = repository;
		}

		#endregion Public Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Gets a <see cref="IPlugin" /> by name.
		/// </summary>
		/// <param name="name">The name of the <see cref="IPlugin" /> to lookup.</param>
		/// <returns>
		/// The <see cref="IPlugin" /> from the map with the name specified, or 
		/// <c>null</c> if no plugin is found.
		/// </returns>
		/// <remarks>
		/// <para>
		/// Lookup a plugin by name. If the plugin is not found <c>null</c>
		/// will be returned.
		/// </para>
		/// </remarks>
		public IPlugin this[string name]
		{
			get
			{
				if (name == null)
				{
					throw new ArgumentNullException("name");
				}

				lock(this)
				{
					return (IPlugin)m_mapName2Plugin[name];
				}
			}
		}

		/// <summary>
		/// Gets all possible plugins as a list of <see cref="IPlugin" /> objects.
		/// </summary>
		/// <value>All possible plugins as a list of <see cref="IPlugin" /> objects.</value>
		/// <remarks>
		/// <para>
		/// Get a collection of all the plugins defined in this map.
		/// </para>
		/// </remarks>
		public PluginCollection AllPlugins
		{
			get
			{
				lock(this)
				{
					return new PluginCollection(m_mapName2Plugin.Values);
				}
			}
		}
		
		#endregion Public Instance Properties

		#region Public Instance Methods

		/// <summary>
		/// Adds a <see cref="IPlugin" /> to the map.
		/// </summary>
		/// <param name="plugin">The <see cref="IPlugin" /> to add to the map.</param>
		/// <remarks>
		/// <para>
		/// The <see cref="IPlugin" /> will be attached to the repository when added.
		/// </para>
		/// <para>
		/// If there already exists a plugin with the same name 
		/// attached to the repository then the old plugin will
		/// be <see cref="IPlugin.Shutdown"/> and replaced with
		/// the new plugin.
		/// </para>
		/// </remarks>
		public void Add(IPlugin plugin)
		{
			if (plugin == null)
			{
				throw new ArgumentNullException("plugin");
			}

			IPlugin curPlugin = null;

			lock(this)
			{
				// Get the current plugin if it exists
				curPlugin = m_mapName2Plugin[plugin.Name] as IPlugin;

				// Store new plugin
				m_mapName2Plugin[plugin.Name] = plugin;
			}

			// Shutdown existing plugin with same name
			if (curPlugin != null)
			{
				curPlugin.Shutdown();
			}

			// Attach new plugin to repository
			plugin.Attach(m_repository);
		}

		/// <summary>
		/// Removes a <see cref="IPlugin" /> from the map.
		/// </summary>
		/// <param name="plugin">The <see cref="IPlugin" /> to remove from the map.</param>
		/// <remarks>
		/// <para>
		/// Remove a specific plugin from this map.
		/// </para>
		/// </remarks>
		public void Remove(IPlugin plugin)
		{
			if (plugin == null)
			{
				throw new ArgumentNullException("plugin");
			}
			lock(this)
			{
				m_mapName2Plugin.Remove(plugin.Name);
			}
		}

		#endregion Public Instance Methods

		#region Private Instance Fields

		private readonly Hashtable m_mapName2Plugin = new Hashtable();
		private readonly ILoggerRepository m_repository;

		#endregion Private Instance Fields
	}
}
