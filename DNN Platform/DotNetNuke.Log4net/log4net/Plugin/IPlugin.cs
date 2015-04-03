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

using log4net.Repository;

namespace log4net.Plugin
{
	/// <summary>
	/// Interface implemented by logger repository plugins.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Plugins define additional behavior that can be associated
	/// with a <see cref="log4net.Repository.ILoggerRepository"/>.
	/// The <see cref="PluginMap"/> held by the <see cref="log4net.Repository.ILoggerRepository.PluginMap"/>
	/// property is used to store the plugins for a repository.
	/// </para>
	/// <para>
	/// The <c>log4net.Config.PluginAttribute</c> can be used to
	/// attach plugins to repositories created using configuration
	/// attributes.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public interface IPlugin
	{
		/// <summary>
		/// Gets the name of the plugin.
		/// </summary>
		/// <value>
		/// The name of the plugin.
		/// </value>
		/// <remarks>
		/// <para>
		/// Plugins are stored in the <see cref="PluginMap"/>
		/// keyed by name. Each plugin instance attached to a
		/// repository must be a unique name.
		/// </para>
		/// </remarks>
		string Name { get; }

		/// <summary>
		/// Attaches the plugin to the specified <see cref="ILoggerRepository"/>.
		/// </summary>
		/// <param name="repository">The <see cref="ILoggerRepository"/> that this plugin should be attached to.</param>
		/// <remarks>
		/// <para>
		/// A plugin may only be attached to a single repository.
		/// </para>
		/// <para>
		/// This method is called when the plugin is attached to the repository.
		/// </para>
		/// </remarks>
		void Attach(ILoggerRepository repository);

		/// <summary>
		/// Is called when the plugin is to shutdown.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method is called to notify the plugin that 
		/// it should stop operating and should detach from
		/// the repository.
		/// </para>
		/// </remarks>
		void Shutdown();
	}
}
