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
using log4net;
using log4net.ObjectRenderer;
using log4net.Core;
using log4net.Plugin;
using log4net.Repository.Hierarchy;
using log4net.Util;

namespace log4net.Repository
{
	#region LoggerRepositoryShutdownEvent

	/// <summary>
	/// Delegate used to handle logger repository shutdown event notifications
	/// </summary>
	/// <param name="sender">The <see cref="ILoggerRepository"/> that is shutting down.</param>
	/// <param name="e">Empty event args</param>
	/// <remarks>
	/// <para>
	/// Delegate used to handle logger repository shutdown event notifications.
	/// </para>
	/// </remarks>
	public delegate void LoggerRepositoryShutdownEventHandler(object sender, EventArgs e);

	#endregion

	#region LoggerRepositoryConfigurationResetEventHandler

	/// <summary>
	/// Delegate used to handle logger repository configuration reset event notifications
	/// </summary>
	/// <param name="sender">The <see cref="ILoggerRepository"/> that has had its configuration reset.</param>
	/// <param name="e">Empty event args</param>
	/// <remarks>
	/// <para>
	/// Delegate used to handle logger repository configuration reset event notifications.
	/// </para>
	/// </remarks>
	public delegate void LoggerRepositoryConfigurationResetEventHandler(object sender, EventArgs e);

	#endregion

	#region LoggerRepositoryConfigurationChangedEventHandler

	/// <summary>
	/// Delegate used to handle event notifications for logger repository configuration changes.
	/// </summary>
	/// <param name="sender">The <see cref="ILoggerRepository"/> that has had its configuration changed.</param>
	/// <param name="e">Empty event arguments.</param>
	/// <remarks>
	/// <para>
	/// Delegate used to handle event notifications for logger repository configuration changes.
	/// </para>
	/// </remarks>
	public delegate void LoggerRepositoryConfigurationChangedEventHandler(object sender, EventArgs e);

    #endregion
	
	/// <summary>
	/// Interface implemented by logger repositories.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This interface is implemented by logger repositories. e.g. 
	/// <see cref="Hierarchy"/>.
	/// </para>
	/// <para>
	/// This interface is used by the <see cref="LogManager"/>
	/// to obtain <see cref="ILog"/> interfaces.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public interface ILoggerRepository
	{
		/// <summary>
		/// The name of the repository
		/// </summary>
		/// <value>
		/// The name of the repository
		/// </value>
		/// <remarks>
		/// <para>
		/// The name of the repository.
		/// </para>
		/// </remarks>
		string Name { get; set; }

		/// <summary>
		/// RendererMap accesses the object renderer map for this repository.
		/// </summary>
		/// <value>
		/// RendererMap accesses the object renderer map for this repository.
		/// </value>
		/// <remarks>
		/// <para>
		/// RendererMap accesses the object renderer map for this repository.
		/// </para>
		/// <para>
		/// The RendererMap holds a mapping between types and
		/// <see cref="IObjectRenderer"/> objects.
		/// </para>
		/// </remarks>
		RendererMap RendererMap { get; }

		/// <summary>
		/// The plugin map for this repository.
		/// </summary>
		/// <value>
		/// The plugin map for this repository.
		/// </value>
		/// <remarks>
		/// <para>
		/// The plugin map holds the <see cref="IPlugin"/> instances
		/// that have been attached to this repository.
		/// </para>
		/// </remarks>
		PluginMap PluginMap { get; }

		/// <summary>
		/// Get the level map for the Repository.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Get the level map for the Repository.
		/// </para>
		/// <para>
		/// The level map defines the mappings between
		/// level names and <see cref="Level"/> objects in
		/// this repository.
		/// </para>
		/// </remarks>
		LevelMap LevelMap { get; }

		/// <summary>
		/// The threshold for all events in this repository
		/// </summary>
		/// <value>
		/// The threshold for all events in this repository
		/// </value>
		/// <remarks>
		/// <para>
		/// The threshold for all events in this repository.
		/// </para>
		/// </remarks>
		Level Threshold { get; set; }

		/// <summary>
		/// Check if the named logger exists in the repository. If so return
		/// its reference, otherwise returns <c>null</c>.
		/// </summary>
		/// <param name="name">The name of the logger to lookup</param>
		/// <returns>The Logger object with the name specified</returns>
		/// <remarks>
		/// <para>
		/// If the names logger exists it is returned, otherwise
		/// <c>null</c> is returned.
		/// </para>
		/// </remarks>
		ILogger Exists(string name);

		/// <summary>
		/// Returns all the currently defined loggers as an Array.
		/// </summary>
		/// <returns>All the defined loggers</returns>
		/// <remarks>
		/// <para>
		/// Returns all the currently defined loggers as an Array.
		/// </para>
		/// </remarks>
		ILogger[] GetCurrentLoggers();

		/// <summary>
		/// Returns a named logger instance
		/// </summary>
		/// <param name="name">The name of the logger to retrieve</param>
		/// <returns>The logger object with the name specified</returns>
		/// <remarks>
		/// <para>
		/// Returns a named logger instance.
		/// </para>
		/// <para>
		/// If a logger of that name already exists, then it will be
		/// returned.  Otherwise, a new logger will be instantiated and
		/// then linked with its existing ancestors as well as children.
		/// </para>
		/// </remarks>
		ILogger GetLogger(string name);

		/// <summary>Shutdown the repository</summary>
		/// <remarks>
		/// <para>
		/// Shutting down a repository will <i>safely</i> close and remove
		/// all appenders in all loggers including the root logger.
		/// </para>
		/// <para>
		/// Some appenders need to be closed before the
		/// application exists. Otherwise, pending logging events might be
		/// lost.
		/// </para>
		/// <para>
		/// The <see cref="M:Shutdown()"/> method is careful to close nested
		/// appenders before closing regular appenders. This is allows
		/// configurations where a regular appender is attached to a logger
		/// and again to a nested appender.
		/// </para>
		/// </remarks>
		void Shutdown();

		/// <summary>
		/// Reset the repositories configuration to a default state
		/// </summary>
		/// <remarks>
		/// <para>
		/// Reset all values contained in this instance to their
		/// default state.
		/// </para>
		/// <para>
		/// Existing loggers are not removed. They are just reset.
		/// </para>
		/// <para>
		/// This method should be used sparingly and with care as it will
		/// block all logging until it is completed.
		/// </para>
		/// </remarks>
		void ResetConfiguration();

		/// <summary>
		/// Log the <see cref="LoggingEvent"/> through this repository.
		/// </summary>
		/// <param name="logEvent">the event to log</param>
		/// <remarks>
		/// <para>
		/// This method should not normally be used to log.
		/// The <see cref="ILog"/> interface should be used 
		/// for routine logging. This interface can be obtained
		/// using the <see cref="M:log4net.LogManager.GetLogger(string)"/> method.
		/// </para>
		/// <para>
		/// The <c>logEvent</c> is delivered to the appropriate logger and
		/// that logger is then responsible for logging the event.
		/// </para>
		/// </remarks>
		void Log(LoggingEvent logEvent);

		/// <summary>
		/// Flag indicates if this repository has been configured.
		/// </summary>
		/// <value>
		/// Flag indicates if this repository has been configured.
		/// </value>
		/// <remarks>
		/// <para>
		/// Flag indicates if this repository has been configured.
		/// </para>
		/// </remarks>
		bool Configured { get; set; }

        /// <summary>
        /// Collection of internal messages captured during the most 
        /// recent configuration process.
        /// </summary>
        ICollection ConfigurationMessages { get; set; }

		/// <summary>
		/// Event to notify that the repository has been shutdown.
		/// </summary>
		/// <value>
		/// Event to notify that the repository has been shutdown.
		/// </value>
		/// <remarks>
		/// <para>
		/// Event raised when the repository has been shutdown.
		/// </para>
		/// </remarks>
		event LoggerRepositoryShutdownEventHandler ShutdownEvent;

		/// <summary>
		/// Event to notify that the repository has had its configuration reset.
		/// </summary>
		/// <value>
		/// Event to notify that the repository has had its configuration reset.
		/// </value>
		/// <remarks>
		/// <para>
		/// Event raised when the repository's configuration has been
		/// reset to default.
		/// </para>
		/// </remarks>
		event LoggerRepositoryConfigurationResetEventHandler ConfigurationReset;

		/// <summary>
		/// Event to notify that the repository has had its configuration changed.
		/// </summary>
		/// <value>
		/// Event to notify that the repository has had its configuration changed.
		/// </value>
		/// <remarks>
		/// <para>
		/// Event raised when the repository's configuration has been changed.
		/// </para>
		/// </remarks>
		event LoggerRepositoryConfigurationChangedEventHandler ConfigurationChanged;

		/// <summary>
		/// Repository specific properties
		/// </summary>
		/// <value>
		/// Repository specific properties
		/// </value>
		/// <remarks>
		/// <para>
		/// These properties can be specified on a repository specific basis.
		/// </para>
		/// </remarks>
		PropertiesDictionary Properties { get; }

		/// <summary>
		/// Returns all the Appenders that are configured as an Array.
		/// </summary>
		/// <returns>All the Appenders</returns>
		/// <remarks>
		/// <para>
		/// Returns all the Appenders that are configured as an Array.
		/// </para>
		/// </remarks>
		log4net.Appender.IAppender[] GetAppenders();
	}
}
