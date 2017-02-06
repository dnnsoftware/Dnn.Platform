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
using System.Reflection;

using log4net.Appender;
using log4net.Util;
using log4net.Repository;


namespace log4net.Core
{
	/// <summary>
	/// The implementation of the <see cref="IRepositorySelector"/> interface suitable
	/// for use with the compact framework
	/// </summary>
	/// <remarks>
	/// <para>
	/// This <see cref="IRepositorySelector"/> implementation is a simple
	/// mapping between repository name and <see cref="ILoggerRepository"/>
	/// object.
	/// </para>
	/// <para>
	/// The .NET Compact Framework 1.0 does not support retrieving assembly
	/// level attributes therefore unlike the <c>DefaultRepositorySelector</c>
	/// this selector does not examine the calling assembly for attributes.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	public class CompactRepositorySelector : IRepositorySelector
	{
		#region Member Variables

		private const string DefaultRepositoryName = "log4net-default-repository";

		private readonly Hashtable m_name2repositoryMap = new Hashtable();
		private readonly Type m_defaultRepositoryType;

		private event LoggerRepositoryCreationEventHandler m_loggerRepositoryCreatedEvent;

		#endregion

		#region Constructors

		/// <summary>
		/// Create a new repository selector
		/// </summary>
		/// <param name="defaultRepositoryType">the type of the repositories to create, must implement <see cref="ILoggerRepository"/></param>
		/// <remarks>
		/// <para>
		/// Create an new compact repository selector.
		/// The default type for repositories must be specified,
		/// an appropriate value would be <see cref="log4net.Repository.Hierarchy.Hierarchy"/>.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">throw if <paramref name="defaultRepositoryType"/> is null</exception>
		/// <exception cref="ArgumentOutOfRangeException">throw if <paramref name="defaultRepositoryType"/> does not implement <see cref="ILoggerRepository"/></exception>
		public CompactRepositorySelector(Type defaultRepositoryType)
		{
			if (defaultRepositoryType == null)
			{
				throw new ArgumentNullException("defaultRepositoryType");
			}

			// Check that the type is a repository
			if (! (typeof(ILoggerRepository).IsAssignableFrom(defaultRepositoryType)) )
			{
				throw log4net.Util.SystemInfo.CreateArgumentOutOfRangeException("defaultRepositoryType", (object)defaultRepositoryType, "Parameter: defaultRepositoryType, Value: ["+defaultRepositoryType+"] out of range. Argument must implement the ILoggerRepository interface");
			}

			m_defaultRepositoryType = defaultRepositoryType;

			LogLog.Debug(declaringType, "defaultRepositoryType ["+m_defaultRepositoryType+"]");
		}

		#endregion

		#region Implementation of IRepositorySelector

		/// <summary>
		/// Get the <see cref="ILoggerRepository"/> for the specified assembly
		/// </summary>
		/// <param name="assembly">not used</param>
		/// <returns>The default <see cref="ILoggerRepository"/></returns>
		/// <remarks>
		/// <para>
		/// The <paramref name="assembly"/> argument is not used. This selector does not create a
		/// separate repository for each assembly. 
		/// </para>
		/// <para>
		/// As a named repository is not specified the default repository is 
		/// returned. The default repository is named <c>log4net-default-repository</c>.
		/// </para>
		/// </remarks>
		public ILoggerRepository GetRepository(Assembly assembly)
		{
			return CreateRepository(assembly, m_defaultRepositoryType);
		}

		/// <summary>
		/// Get the named <see cref="ILoggerRepository"/>
		/// </summary>
		/// <param name="repositoryName">the name of the repository to lookup</param>
		/// <returns>The named <see cref="ILoggerRepository"/></returns>
		/// <remarks>
		/// <para>
		/// Get the named <see cref="ILoggerRepository"/>. The default 
		/// repository is <c>log4net-default-repository</c>. Other repositories 
		/// must be created using the <see cref="M:CreateRepository(string, Type)"/>.
		/// If the named repository does not exist an exception is thrown.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">throw if <paramref name="repositoryName"/> is null</exception>
		/// <exception cref="LogException">throw if the <paramref name="repositoryName"/> does not exist</exception>
		public ILoggerRepository GetRepository(string repositoryName)
		{
			if (repositoryName == null)
			{
				throw new ArgumentNullException("repositoryName");
			}

			lock(this)
			{
				// Lookup in map
				ILoggerRepository rep = m_name2repositoryMap[repositoryName] as ILoggerRepository;
				if (rep == null)
				{
					throw new LogException("Repository ["+repositoryName+"] is NOT defined.");
				}
				return rep;
			}
		}

		/// <summary>
		/// Create a new repository for the assembly specified 
		/// </summary>
		/// <param name="assembly">not used</param>
		/// <param name="repositoryType">the type of repository to create, must implement <see cref="ILoggerRepository"/></param>
		/// <returns>the repository created</returns>
		/// <remarks>
		/// <para>
		/// The <paramref name="assembly"/> argument is not used. This selector does not create a
		/// separate repository for each assembly. 
		/// </para>
		/// <para>
		/// If the <paramref name="repositoryType"/> is <c>null</c> then the
		/// default repository type specified to the constructor is used.
		/// </para>
		/// <para>
		/// As a named repository is not specified the default repository is 
		/// returned. The default repository is named <c>log4net-default-repository</c>.
		/// </para>
		/// </remarks>
		public ILoggerRepository CreateRepository(Assembly assembly, Type repositoryType)
		{
			// If the type is not set then use the default type
			if (repositoryType == null)
			{
				repositoryType = m_defaultRepositoryType;
			}

			lock(this)
			{
				// This method should not throw if the default repository already exists.

				// First check that the repository does not exist
				ILoggerRepository rep = m_name2repositoryMap[DefaultRepositoryName] as ILoggerRepository;
				if (rep == null)
				{
					// Must create the repository
					rep = CreateRepository(DefaultRepositoryName, repositoryType);
				}

				return rep;
			}		
		}

		/// <summary>
		/// Create a new repository for the repository specified
		/// </summary>
		/// <param name="repositoryName">the repository to associate with the <see cref="ILoggerRepository"/></param>
		/// <param name="repositoryType">the type of repository to create, must implement <see cref="ILoggerRepository"/>.
		/// If this param is null then the default repository type is used.</param>
		/// <returns>the repository created</returns>
		/// <remarks>
		/// <para>
		/// The <see cref="ILoggerRepository"/> created will be associated with the repository
		/// specified such that a call to <see cref="M:GetRepository(string)"/> with the
		/// same repository specified will return the same repository instance.
		/// </para>
		/// <para>
		/// If the named repository already exists an exception will be thrown.
		/// </para>
		/// <para>
		/// If <paramref name="repositoryType"/> is <c>null</c> then the default 
		/// repository type specified to the constructor is used.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">throw if <paramref name="repositoryName"/> is null</exception>
		/// <exception cref="LogException">throw if the <paramref name="repositoryName"/> already exists</exception>
		public ILoggerRepository CreateRepository(string repositoryName, Type repositoryType)
		{
			if (repositoryName == null)
			{
				throw new ArgumentNullException("repositoryName");
			}

			// If the type is not set then use the default type
			if (repositoryType == null)
			{
				repositoryType = m_defaultRepositoryType;
			}

			lock(this)
			{
				ILoggerRepository rep = null;

				// First check that the repository does not exist
				rep = m_name2repositoryMap[repositoryName] as ILoggerRepository;
				if (rep != null)
				{
					throw new LogException("Repository ["+repositoryName+"] is already defined. Repositories cannot be redefined.");
				}
				else
				{
					LogLog.Debug(declaringType, "Creating repository ["+repositoryName+"] using type ["+repositoryType+"]");

					// Call the no arg constructor for the repositoryType
					rep = (ILoggerRepository)Activator.CreateInstance(repositoryType);

					// Set the name of the repository
					rep.Name = repositoryName;

					// Store in map
					m_name2repositoryMap[repositoryName] = rep;

					// Notify listeners that the repository has been created
					OnLoggerRepositoryCreatedEvent(rep);
				}

				return rep;
			}
		}

		/// <summary>
		/// Test if a named repository exists
		/// </summary>
		/// <param name="repositoryName">the named repository to check</param>
		/// <returns><c>true</c> if the repository exists</returns>
		/// <remarks>
		/// <para>
		/// Test if a named repository exists. Use <see cref="M:CreateRepository(string, Type)"/>
		/// to create a new repository and <see cref="M:GetRepository(string)"/> to retrieve 
		/// a repository.
		/// </para>
		/// </remarks>
		public bool ExistsRepository(string repositoryName)
		{
			lock(this)
			{
				return m_name2repositoryMap.ContainsKey(repositoryName);
			}
		}

		/// <summary>
		/// Gets a list of <see cref="ILoggerRepository"/> objects
		/// </summary>
		/// <returns>an array of all known <see cref="ILoggerRepository"/> objects</returns>
		/// <remarks>
		/// <para>
		/// Gets an array of all of the repositories created by this selector.
		/// </para>
		/// </remarks>
		public ILoggerRepository[] GetAllRepositories()
		{
			lock(this)
			{
				ICollection reps = m_name2repositoryMap.Values;
				ILoggerRepository[] all = new ILoggerRepository[reps.Count];
				reps.CopyTo(all, 0);
				return all;
			}
		}

		#endregion

	    #region Private Static Fields

	    /// <summary>
	    /// The fully qualified type of the CompactRepositorySelector class.
	    /// </summary>
	    /// <remarks>
	    /// Used by the internal logger to record the Type of the
	    /// log message.
	    /// </remarks>
	    private readonly static Type declaringType = typeof(CompactRepositorySelector);

	    #endregion Private Static Fields

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
		public event LoggerRepositoryCreationEventHandler LoggerRepositoryCreatedEvent
		{
			add { m_loggerRepositoryCreatedEvent += value; }
			remove { m_loggerRepositoryCreatedEvent -= value; }
		}

		/// <summary>
		/// Notify the registered listeners that the repository has been created
		/// </summary>
		/// <param name="repository">The repository that has been created</param>
		/// <remarks>
		/// <para>
		/// Raises the <event cref="LoggerRepositoryCreatedEvent">LoggerRepositoryCreatedEvent</event>
		/// event.
		/// </para>
		/// </remarks>
		protected virtual void OnLoggerRepositoryCreatedEvent(ILoggerRepository repository)
		{
			LoggerRepositoryCreationEventHandler handler = m_loggerRepositoryCreatedEvent;
			if (handler != null)
			{
				handler(this, new LoggerRepositoryCreationEventArgs(repository));
			}
		}
	}
}
