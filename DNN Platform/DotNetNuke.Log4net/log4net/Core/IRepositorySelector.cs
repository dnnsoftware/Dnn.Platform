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
using System.Reflection;

using log4net.Repository;

namespace log4net.Core
{
	#region LoggerRepositoryCreationEvent

	/// <summary>
	/// Delegate used to handle logger repository creation event notifications
	/// </summary>
	/// <param name="sender">The <see cref="IRepositorySelector"/> which created the repository.</param>
	/// <param name="e">The <see cref="LoggerRepositoryCreationEventArgs"/> event args
	/// that holds the <see cref="ILoggerRepository"/> instance that has been created.</param>
	/// <remarks>
	/// <para>
	/// Delegate used to handle logger repository creation event notifications.
	/// </para>
	/// </remarks>
	public delegate void LoggerRepositoryCreationEventHandler(object sender, LoggerRepositoryCreationEventArgs e);

	/// <summary>
	/// Provides data for the <see cref="IRepositorySelector.LoggerRepositoryCreatedEvent"/> event.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A <see cref="IRepositorySelector.LoggerRepositoryCreatedEvent"/> 
	/// event is raised every time a <see cref="ILoggerRepository"/> is created.
	/// </para>
	/// </remarks>
	public class LoggerRepositoryCreationEventArgs : EventArgs
	{
		/// <summary>
		/// The <see cref="ILoggerRepository"/> created
		/// </summary>
		private ILoggerRepository m_repository;

		/// <summary>
		/// Construct instance using <see cref="ILoggerRepository"/> specified
		/// </summary>
		/// <param name="repository">the <see cref="ILoggerRepository"/> that has been created</param>
		/// <remarks>
		/// <para>
		/// Construct instance using <see cref="ILoggerRepository"/> specified
		/// </para>
		/// </remarks>
		public LoggerRepositoryCreationEventArgs(ILoggerRepository repository)
		{
			m_repository = repository;
		}

		/// <summary>
		/// The <see cref="ILoggerRepository"/> that has been created
		/// </summary>
		/// <value>
		/// The <see cref="ILoggerRepository"/> that has been created
		/// </value>
		/// <remarks>
		/// <para>
		/// The <see cref="ILoggerRepository"/> that has been created
		/// </para>
		/// </remarks>
		public ILoggerRepository LoggerRepository
		{
			get { return m_repository; }
		}
	}

	#endregion

	/// <summary>
	/// Interface used by the <see cref="LogManager"/> to select the <see cref="ILoggerRepository"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The <see cref="LogManager"/> uses a <see cref="IRepositorySelector"/> 
	/// to specify the policy for selecting the correct <see cref="ILoggerRepository"/> 
	/// to return to the caller.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public interface IRepositorySelector
	{
		/// <summary>
		/// Gets the <see cref="ILoggerRepository"/> for the specified assembly.
		/// </summary>
		/// <param name="assembly">The assembly to use to lookup to the <see cref="ILoggerRepository"/></param>
		/// <returns>The <see cref="ILoggerRepository"/> for the assembly.</returns>
		/// <remarks>
		/// <para>
		/// Gets the <see cref="ILoggerRepository"/> for the specified assembly.
		/// </para>
		/// <para>
		/// How the association between <see cref="Assembly"/> and <see cref="ILoggerRepository"/>
		/// is made is not defined. The implementation may choose any method for
		/// this association. The results of this method must be repeatable, i.e.
		/// when called again with the same arguments the result must be the
		/// save value.
		/// </para>
		/// </remarks>
		ILoggerRepository GetRepository(Assembly assembly);

		/// <summary>
		/// Gets the named <see cref="ILoggerRepository"/>.
		/// </summary>
		/// <param name="repositoryName">The name to use to lookup to the <see cref="ILoggerRepository"/>.</param>
		/// <returns>The named <see cref="ILoggerRepository"/></returns>
		/// <remarks>
		/// Lookup a named <see cref="ILoggerRepository"/>. This is the repository created by
		/// calling <see cref="CreateRepository(string,Type)"/>.
		/// </remarks>
		ILoggerRepository GetRepository(string repositoryName);

		/// <summary>
		/// Creates a new repository for the assembly specified.
		/// </summary>
		/// <param name="assembly">The assembly to use to create the domain to associate with the <see cref="ILoggerRepository"/>.</param>
		/// <param name="repositoryType">The type of repository to create, must implement <see cref="ILoggerRepository"/>.</param>
		/// <returns>The repository created.</returns>
		/// <remarks>
		/// <para>
		/// The <see cref="ILoggerRepository"/> created will be associated with the domain
		/// specified such that a call to <see cref="GetRepository(Assembly)"/> with the
		/// same assembly specified will return the same repository instance.
		/// </para>
		/// <para>
		/// How the association between <see cref="Assembly"/> and <see cref="ILoggerRepository"/>
		/// is made is not defined. The implementation may choose any method for
		/// this association.
		/// </para>
		/// </remarks>
		ILoggerRepository CreateRepository(Assembly assembly, Type repositoryType);

		/// <summary>
		/// Creates a new repository with the name specified.
		/// </summary>
		/// <param name="repositoryName">The name to associate with the <see cref="ILoggerRepository"/>.</param>
		/// <param name="repositoryType">The type of repository to create, must implement <see cref="ILoggerRepository"/>.</param>
		/// <returns>The repository created.</returns>
		/// <remarks>
		/// <para>
		/// The <see cref="ILoggerRepository"/> created will be associated with the name
		/// specified such that a call to <see cref="GetRepository(string)"/> with the
		/// same name will return the same repository instance.
		/// </para>
		/// </remarks>
		ILoggerRepository CreateRepository(string repositoryName, Type repositoryType);

		/// <summary>
		/// Test if a named repository exists
		/// </summary>
		/// <param name="repositoryName">the named repository to check</param>
		/// <returns><c>true</c> if the repository exists</returns>
		/// <remarks>
		/// <para>
		/// Test if a named repository exists. Use <see cref="CreateRepository(Assembly, Type)"/>
		/// to create a new repository and <see cref="GetRepository(Assembly)"/> to retrieve 
		/// a repository.
		/// </para>
		/// </remarks>
		bool ExistsRepository(string repositoryName);

		/// <summary>
		/// Gets an array of all currently defined repositories.
		/// </summary>
		/// <returns>
		/// An array of the <see cref="ILoggerRepository"/> instances created by 
		/// this <see cref="IRepositorySelector"/>.</returns>
		/// <remarks>
		/// <para>
		/// Gets an array of all of the repositories created by this selector.
		/// </para>
		/// </remarks>
		ILoggerRepository[] GetAllRepositories();

		/// <summary>
		/// Event to notify that a logger repository has been created.
		/// </summary>
		/// <value>
		/// Event to notify that a logger repository has been created.
		/// </value>
		/// <remarks>
		/// <para>
		/// Event raised when a new repository is created.
		/// The event source will be this selector. The event args will
		/// be a <see cref="LoggerRepositoryCreationEventArgs"/> which
		/// holds the newly created <see cref="ILoggerRepository"/>.
		/// </para>
		/// </remarks>
		event LoggerRepositoryCreationEventHandler LoggerRepositoryCreatedEvent;
	}
}
