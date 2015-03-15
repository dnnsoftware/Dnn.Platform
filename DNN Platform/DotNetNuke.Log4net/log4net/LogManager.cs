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

using log4net.Core;
using log4net.Repository;

namespace log4net
{
	/// <summary>
	/// This class is used by client applications to request logger instances.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This class has static methods that are used by a client to request
	/// a logger instance. The <see cref="GetLogger(string)"/> method is 
	/// used to retrieve a logger.
	/// </para>
	/// <para>
	/// See the <see cref="ILog"/> interface for more details.
	/// </para>
	/// </remarks>
	/// <example>Simple example of logging messages
	/// <code lang="C#">
	/// ILog log = LogManager.GetLogger("application-log");
	/// 
	/// log.Info("Application Start");
	/// log.Debug("This is a debug message");
	/// 
	/// if (log.IsDebugEnabled)
	/// {
	///		log.Debug("This is another debug message");
	/// }
	/// </code>
	/// </example>
	/// <threadsafety static="true" instance="true" />
	/// <seealso cref="ILog"/>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public sealed class LogManager
	{
		#region Private Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="LogManager" /> class. 
		/// </summary>
		/// <remarks>
		/// Uses a private access modifier to prevent instantiation of this class.
		/// </remarks>
		private LogManager()
		{
		}

		#endregion Private Instance Constructors

		#region Type Specific Manager Methods

		/// <overloads>Returns the named logger if it exists.</overloads>
		/// <summary>
		/// Returns the named logger if it exists.
		/// </summary>
		/// <remarks>
		/// <para>
		/// If the named logger exists (in the default repository) then it
		/// returns a reference to the logger, otherwise it returns <c>null</c>.
		/// </para>
		/// </remarks>
		/// <param name="name">The fully qualified logger name to look for.</param>
		/// <returns>The logger found, or <c>null</c> if no logger could be found.</returns>
		public static ILog Exists(string name) 
		{
			return Exists(Assembly.GetCallingAssembly(), name);
		}

		/// <summary>
		/// Returns the named logger if it exists.
		/// </summary>
		/// <remarks>
		/// <para>
		/// If the named logger exists (in the specified repository) then it
		/// returns a reference to the logger, otherwise it returns
		/// <c>null</c>.
		/// </para>
		/// </remarks>
		/// <param name="repository">The repository to lookup in.</param>
		/// <param name="name">The fully qualified logger name to look for.</param>
		/// <returns>
		/// The logger found, or <c>null</c> if the logger doesn't exist in the specified 
		/// repository.
		/// </returns>
		public static ILog Exists(string repository, string name) 
		{
			return WrapLogger(LoggerManager.Exists(repository, name));
		}

		/// <summary>
		/// Returns the named logger if it exists.
		/// </summary>
		/// <remarks>
		/// <para>
		/// If the named logger exists (in the repository for the specified assembly) then it
		/// returns a reference to the logger, otherwise it returns
		/// <c>null</c>.
		/// </para>
		/// </remarks>
		/// <param name="repositoryAssembly">The assembly to use to lookup the repository.</param>
		/// <param name="name">The fully qualified logger name to look for.</param>
		/// <returns>
		/// The logger, or <c>null</c> if the logger doesn't exist in the specified
		/// assembly's repository.
		/// </returns>
		public static ILog Exists(Assembly repositoryAssembly, string name) 
		{
			return WrapLogger(LoggerManager.Exists(repositoryAssembly, name));
		}

		/// <overloads>Get the currently defined loggers.</overloads>
		/// <summary>
		/// Returns all the currently defined loggers in the default repository.
		/// </summary>
		/// <remarks>
		/// <para>The root logger is <b>not</b> included in the returned array.</para>
		/// </remarks>
		/// <returns>All the defined loggers.</returns>
		public static ILog[] GetCurrentLoggers()
		{
			return GetCurrentLoggers(Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Returns all the currently defined loggers in the specified repository.
		/// </summary>
		/// <param name="repository">The repository to lookup in.</param>
		/// <remarks>
		/// The root logger is <b>not</b> included in the returned array.
		/// </remarks>
		/// <returns>All the defined loggers.</returns>
		public static ILog[] GetCurrentLoggers(string repository)
		{
			return WrapLoggers(LoggerManager.GetCurrentLoggers(repository));
		}

		/// <summary>
		/// Returns all the currently defined loggers in the specified assembly's repository.
		/// </summary>
		/// <param name="repositoryAssembly">The assembly to use to lookup the repository.</param>
		/// <remarks>
		/// The root logger is <b>not</b> included in the returned array.
		/// </remarks>
		/// <returns>All the defined loggers.</returns>
		public static ILog[] GetCurrentLoggers(Assembly repositoryAssembly)
		{
			return WrapLoggers(LoggerManager.GetCurrentLoggers(repositoryAssembly));
		}

		/// <overloads>Get or create a logger.</overloads>
		/// <summary>
		/// Retrieves or creates a named logger.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Retrieves a logger named as the <paramref name="name"/>
		/// parameter. If the named logger already exists, then the
		/// existing instance will be returned. Otherwise, a new instance is
		/// created.
		/// </para>
		/// <para>By default, loggers do not have a set level but inherit
		/// it from the hierarchy. This is one of the central features of
		/// log4net.
		/// </para>
		/// </remarks>
		/// <param name="name">The name of the logger to retrieve.</param>
		/// <returns>The logger with the name specified.</returns>
		public static ILog GetLogger(string name)
		{
			return GetLogger(Assembly.GetCallingAssembly(), name);
		}

		/// <summary>
		/// Retrieves or creates a named logger.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Retrieve a logger named as the <paramref name="name"/>
		/// parameter. If the named logger already exists, then the
		/// existing instance will be returned. Otherwise, a new instance is
		/// created.
		/// </para>
		/// <para>
		/// By default, loggers do not have a set level but inherit
		/// it from the hierarchy. This is one of the central features of
		/// log4net.
		/// </para>
		/// </remarks>
		/// <param name="repository">The repository to lookup in.</param>
		/// <param name="name">The name of the logger to retrieve.</param>
		/// <returns>The logger with the name specified.</returns>
		public static ILog GetLogger(string repository, string name)
		{
			return WrapLogger(LoggerManager.GetLogger(repository, name));
		}

		/// <summary>
		/// Retrieves or creates a named logger.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Retrieve a logger named as the <paramref name="name"/>
		/// parameter. If the named logger already exists, then the
		/// existing instance will be returned. Otherwise, a new instance is
		/// created.
		/// </para>
		/// <para>
		/// By default, loggers do not have a set level but inherit
		/// it from the hierarchy. This is one of the central features of
		/// log4net.
		/// </para>
		/// </remarks>
		/// <param name="repositoryAssembly">The assembly to use to lookup the repository.</param>
		/// <param name="name">The name of the logger to retrieve.</param>
		/// <returns>The logger with the name specified.</returns>
		public static ILog GetLogger(Assembly repositoryAssembly, string name)
		{
			return WrapLogger(LoggerManager.GetLogger(repositoryAssembly, name));
		}	

		/// <summary>
		/// Shorthand for <see cref="LogManager.GetLogger(string)"/>.
		/// </summary>
		/// <remarks>
		/// Get the logger for the fully qualified name of the type specified.
		/// </remarks>
		/// <param name="type">The full name of <paramref name="type"/> will be used as the name of the logger to retrieve.</param>
		/// <returns>The logger with the name specified.</returns>
		public static ILog GetLogger(Type type) 
		{
			return GetLogger(Assembly.GetCallingAssembly(), type.FullName);
		}

		/// <summary>
		/// Shorthand for <see cref="LogManager.GetLogger(string)"/>.
		/// </summary>
		/// <remarks>
		/// Gets the logger for the fully qualified name of the type specified.
		/// </remarks>
		/// <param name="repository">The repository to lookup in.</param>
		/// <param name="type">The full name of <paramref name="type"/> will be used as the name of the logger to retrieve.</param>
		/// <returns>The logger with the name specified.</returns>
		public static ILog GetLogger(string repository, Type type) 
		{
			return WrapLogger(LoggerManager.GetLogger(repository, type));
		}

		/// <summary>
		/// Shorthand for <see cref="LogManager.GetLogger(string)"/>.
		/// </summary>
		/// <remarks>
		/// Gets the logger for the fully qualified name of the type specified.
		/// </remarks>
		/// <param name="repositoryAssembly">The assembly to use to lookup the repository.</param>
		/// <param name="type">The full name of <paramref name="type"/> will be used as the name of the logger to retrieve.</param>
		/// <returns>The logger with the name specified.</returns>
		public static ILog GetLogger(Assembly repositoryAssembly, Type type) 
		{
			return WrapLogger(LoggerManager.GetLogger(repositoryAssembly, type));
		}

		#endregion Type Specific Manager Methods

		#region Domain & Repository Manager Methods

		/// <summary>
		/// Shuts down the log4net system.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Calling this method will <b>safely</b> close and remove all
		/// appenders in all the loggers including root contained in all the
		/// default repositories.
		/// </para>
		/// <para>
		/// Some appenders need to be closed before the application exists. 
		/// Otherwise, pending logging events might be lost.
		/// </para>
		/// <para>The <c>shutdown</c> method is careful to close nested
		/// appenders before closing regular appenders. This is allows
		/// configurations where a regular appender is attached to a logger
		/// and again to a nested appender.
		/// </para>
		/// </remarks>
		public static void Shutdown() 
		{
			LoggerManager.Shutdown();
		}

		/// <overloads>Shutdown a logger repository.</overloads>
		/// <summary>
		/// Shuts down the default repository.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Calling this method will <b>safely</b> close and remove all
		/// appenders in all the loggers including root contained in the
		/// default repository.
		/// </para>
		/// <para>Some appenders need to be closed before the application exists. 
		/// Otherwise, pending logging events might be lost.
		/// </para>
		/// <para>The <c>shutdown</c> method is careful to close nested
		/// appenders before closing regular appenders. This is allows
		/// configurations where a regular appender is attached to a logger
		/// and again to a nested appender.
		/// </para>
		/// </remarks>
		public static void ShutdownRepository() 
		{
			ShutdownRepository(Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Shuts down the repository for the repository specified.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Calling this method will <b>safely</b> close and remove all
		/// appenders in all the loggers including root contained in the
		/// <paramref name="repository"/> specified.
		/// </para>
		/// <para>
		/// Some appenders need to be closed before the application exists. 
		/// Otherwise, pending logging events might be lost.
		/// </para>
		/// <para>The <c>shutdown</c> method is careful to close nested
		/// appenders before closing regular appenders. This is allows
		/// configurations where a regular appender is attached to a logger
		/// and again to a nested appender.
		/// </para>
		/// </remarks>
		/// <param name="repository">The repository to shutdown.</param>
		public static void ShutdownRepository(string repository) 
		{
			LoggerManager.ShutdownRepository(repository);
		}

		/// <summary>
		/// Shuts down the repository specified.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Calling this method will <b>safely</b> close and remove all
		/// appenders in all the loggers including root contained in the
		/// repository. The repository is looked up using
		/// the <paramref name="repositoryAssembly"/> specified.
		/// </para>
		/// <para>
		/// Some appenders need to be closed before the application exists. 
		/// Otherwise, pending logging events might be lost.
		/// </para>
		/// <para>
		/// The <c>shutdown</c> method is careful to close nested
		/// appenders before closing regular appenders. This is allows
		/// configurations where a regular appender is attached to a logger
		/// and again to a nested appender.
		/// </para>
		/// </remarks>
		/// <param name="repositoryAssembly">The assembly to use to lookup the repository.</param>
		public static void ShutdownRepository(Assembly repositoryAssembly) 
		{
			LoggerManager.ShutdownRepository(repositoryAssembly);
		}

		/// <overloads>Reset the configuration of a repository</overloads>
		/// <summary>
		/// Resets all values contained in this repository instance to their defaults.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Resets all values contained in the repository instance to their
		/// defaults.  This removes all appenders from all loggers, sets
		/// the level of all non-root loggers to <c>null</c>,
		/// sets their additivity flag to <c>true</c> and sets the level
		/// of the root logger to <see cref="Level.Debug"/>. Moreover,
		/// message disabling is set to its default "off" value.
		/// </para>		
		/// </remarks>
		public static void ResetConfiguration() 
		{
			ResetConfiguration(Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Resets all values contained in this repository instance to their defaults.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Reset all values contained in the repository instance to their
		/// defaults.  This removes all appenders from all loggers, sets
		/// the level of all non-root loggers to <c>null</c>,
		/// sets their additivity flag to <c>true</c> and sets the level
		/// of the root logger to <see cref="Level.Debug"/>. Moreover,
		/// message disabling is set to its default "off" value.
		/// </para>		
		/// </remarks>
		/// <param name="repository">The repository to reset.</param>
		public static void ResetConfiguration(string repository) 
		{
			LoggerManager.ResetConfiguration(repository);
		}

		/// <summary>
		/// Resets all values contained in this repository instance to their defaults.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Reset all values contained in the repository instance to their
		/// defaults.  This removes all appenders from all loggers, sets
		/// the level of all non-root loggers to <c>null</c>,
		/// sets their additivity flag to <c>true</c> and sets the level
		/// of the root logger to <see cref="Level.Debug"/>. Moreover,
		/// message disabling is set to its default "off" value.
		/// </para>		
		/// </remarks>
		/// <param name="repositoryAssembly">The assembly to use to lookup the repository to reset.</param>
		public static void ResetConfiguration(Assembly repositoryAssembly) 
		{
			LoggerManager.ResetConfiguration(repositoryAssembly);
		}

		/// <overloads>Get the logger repository.</overloads>
		/// <summary>
		/// Returns the default <see cref="ILoggerRepository"/> instance.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Gets the <see cref="ILoggerRepository"/> for the repository specified
		/// by the callers assembly (<see cref="Assembly.GetCallingAssembly()"/>).
		/// </para>
		/// </remarks>
		/// <returns>The <see cref="ILoggerRepository"/> instance for the default repository.</returns>
		[Obsolete("Use GetRepository instead of GetLoggerRepository")]
		public static ILoggerRepository GetLoggerRepository()
		{
			return GetRepository(Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Returns the default <see cref="ILoggerRepository"/> instance.
		/// </summary>
		/// <returns>The default <see cref="ILoggerRepository"/> instance.</returns>
		/// <remarks>
		/// <para>
		/// Gets the <see cref="ILoggerRepository"/> for the repository specified
		/// by the <paramref name="repository"/> argument.
		/// </para>
		/// </remarks>
		/// <param name="repository">The repository to lookup in.</param>
		[Obsolete("Use GetRepository instead of GetLoggerRepository")]
		public static ILoggerRepository GetLoggerRepository(string repository)
		{
			return GetRepository(repository);
		}

		/// <summary>
		/// Returns the default <see cref="ILoggerRepository"/> instance.
		/// </summary>
		/// <returns>The default <see cref="ILoggerRepository"/> instance.</returns>
		/// <remarks>
		/// <para>
		/// Gets the <see cref="ILoggerRepository"/> for the repository specified
		/// by the <paramref name="repositoryAssembly"/> argument.
		/// </para>
		/// </remarks>
		/// <param name="repositoryAssembly">The assembly to use to lookup the repository.</param>
		[Obsolete("Use GetRepository instead of GetLoggerRepository")]
		public static ILoggerRepository GetLoggerRepository(Assembly repositoryAssembly)
		{
			return GetRepository(repositoryAssembly);
		}

		/// <overloads>Get a logger repository.</overloads>
		/// <summary>
		/// Returns the default <see cref="ILoggerRepository"/> instance.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Gets the <see cref="ILoggerRepository"/> for the repository specified
		/// by the callers assembly (<see cref="Assembly.GetCallingAssembly()"/>).
		/// </para>
		/// </remarks>
		/// <returns>The <see cref="ILoggerRepository"/> instance for the default repository.</returns>
		public static ILoggerRepository GetRepository()
		{
			return GetRepository(Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Returns the default <see cref="ILoggerRepository"/> instance.
		/// </summary>
		/// <returns>The default <see cref="ILoggerRepository"/> instance.</returns>
		/// <remarks>
		/// <para>
		/// Gets the <see cref="ILoggerRepository"/> for the repository specified
		/// by the <paramref name="repository"/> argument.
		/// </para>
		/// </remarks>
		/// <param name="repository">The repository to lookup in.</param>
		public static ILoggerRepository GetRepository(string repository)
		{
			return LoggerManager.GetRepository(repository);
		}

		/// <summary>
		/// Returns the default <see cref="ILoggerRepository"/> instance.
		/// </summary>
		/// <returns>The default <see cref="ILoggerRepository"/> instance.</returns>
		/// <remarks>
		/// <para>
		/// Gets the <see cref="ILoggerRepository"/> for the repository specified
		/// by the <paramref name="repositoryAssembly"/> argument.
		/// </para>
		/// </remarks>
		/// <param name="repositoryAssembly">The assembly to use to lookup the repository.</param>
		public static ILoggerRepository GetRepository(Assembly repositoryAssembly)
		{
			return LoggerManager.GetRepository(repositoryAssembly);
		}

		/// <overloads>Create a domain</overloads>
		/// <summary>
		/// Creates a repository with the specified repository type.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <b>CreateDomain is obsolete. Use CreateRepository instead of CreateDomain.</b>
		/// </para>
		/// <para>
		/// The <see cref="ILoggerRepository"/> created will be associated with the repository
		/// specified such that a call to <see cref="GetRepository()"/> will return 
		/// the same repository instance.
		/// </para>
		/// </remarks>
		/// <param name="repositoryType">A <see cref="Type"/> that implements <see cref="ILoggerRepository"/>
		/// and has a no arg constructor. An instance of this type will be created to act
		/// as the <see cref="ILoggerRepository"/> for the repository specified.</param>
		/// <returns>The <see cref="ILoggerRepository"/> created for the repository.</returns>
		[Obsolete("Use CreateRepository instead of CreateDomain")]
		public static ILoggerRepository CreateDomain(Type repositoryType)
		{
			return CreateRepository(Assembly.GetCallingAssembly(), repositoryType);
		}

		/// <overloads>Create a logger repository.</overloads>
		/// <summary>
		/// Creates a repository with the specified repository type.
		/// </summary>
		/// <param name="repositoryType">A <see cref="Type"/> that implements <see cref="ILoggerRepository"/>
		/// and has a no arg constructor. An instance of this type will be created to act
		/// as the <see cref="ILoggerRepository"/> for the repository specified.</param>
		/// <returns>The <see cref="ILoggerRepository"/> created for the repository.</returns>
		/// <remarks>
		/// <para>
		/// The <see cref="ILoggerRepository"/> created will be associated with the repository
		/// specified such that a call to <see cref="GetRepository()"/> will return 
		/// the same repository instance.
		/// </para>
		/// </remarks>
		public static ILoggerRepository CreateRepository(Type repositoryType)
		{
			return CreateRepository(Assembly.GetCallingAssembly(), repositoryType);
		}

		/// <summary>
		/// Creates a repository with the specified name.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <b>CreateDomain is obsolete. Use CreateRepository instead of CreateDomain.</b>
		/// </para>
		/// <para>
		/// Creates the default type of <see cref="ILoggerRepository"/> which is a
		/// <see cref="log4net.Repository.Hierarchy.Hierarchy"/> object.
		/// </para>
		/// <para>
		/// The <paramref name="repository"/> name must be unique. Repositories cannot be redefined.
		/// An <see cref="Exception"/> will be thrown if the repository already exists.
		/// </para>
		/// </remarks>
		/// <param name="repository">The name of the repository, this must be unique amongst repositories.</param>
		/// <returns>The <see cref="ILoggerRepository"/> created for the repository.</returns>
		/// <exception cref="LogException">The specified repository already exists.</exception>
		[Obsolete("Use CreateRepository instead of CreateDomain")]
		public static ILoggerRepository CreateDomain(string repository)
		{
			return LoggerManager.CreateRepository(repository);
		}

		/// <summary>
		/// Creates a repository with the specified name.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Creates the default type of <see cref="ILoggerRepository"/> which is a
		/// <see cref="log4net.Repository.Hierarchy.Hierarchy"/> object.
		/// </para>
		/// <para>
		/// The <paramref name="repository"/> name must be unique. Repositories cannot be redefined.
		/// An <see cref="Exception"/> will be thrown if the repository already exists.
		/// </para>
		/// </remarks>
		/// <param name="repository">The name of the repository, this must be unique amongst repositories.</param>
		/// <returns>The <see cref="ILoggerRepository"/> created for the repository.</returns>
		/// <exception cref="LogException">The specified repository already exists.</exception>
		public static ILoggerRepository CreateRepository(string repository)
		{
			return LoggerManager.CreateRepository(repository);
		}

		/// <summary>
		/// Creates a repository with the specified name and repository type.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <b>CreateDomain is obsolete. Use CreateRepository instead of CreateDomain.</b>
		/// </para>
		/// <para>
		/// The <paramref name="repository"/> name must be unique. Repositories cannot be redefined.
		/// An <see cref="Exception"/> will be thrown if the repository already exists.
		/// </para>
		/// </remarks>
		/// <param name="repository">The name of the repository, this must be unique to the repository.</param>
		/// <param name="repositoryType">A <see cref="Type"/> that implements <see cref="ILoggerRepository"/>
		/// and has a no arg constructor. An instance of this type will be created to act
		/// as the <see cref="ILoggerRepository"/> for the repository specified.</param>
		/// <returns>The <see cref="ILoggerRepository"/> created for the repository.</returns>
		/// <exception cref="LogException">The specified repository already exists.</exception>
		[Obsolete("Use CreateRepository instead of CreateDomain")]
		public static ILoggerRepository CreateDomain(string repository, Type repositoryType)
		{
			return LoggerManager.CreateRepository(repository, repositoryType);
		}

		/// <summary>
		/// Creates a repository with the specified name and repository type.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The <paramref name="repository"/> name must be unique. Repositories cannot be redefined.
		/// An <see cref="Exception"/> will be thrown if the repository already exists.
		/// </para>
		/// </remarks>
		/// <param name="repository">The name of the repository, this must be unique to the repository.</param>
		/// <param name="repositoryType">A <see cref="Type"/> that implements <see cref="ILoggerRepository"/>
		/// and has a no arg constructor. An instance of this type will be created to act
		/// as the <see cref="ILoggerRepository"/> for the repository specified.</param>
		/// <returns>The <see cref="ILoggerRepository"/> created for the repository.</returns>
		/// <exception cref="LogException">The specified repository already exists.</exception>
		public static ILoggerRepository CreateRepository(string repository, Type repositoryType)
		{
			return LoggerManager.CreateRepository(repository, repositoryType);
		}

		/// <summary>
		/// Creates a repository for the specified assembly and repository type.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <b>CreateDomain is obsolete. Use CreateRepository instead of CreateDomain.</b>
		/// </para>
		/// <para>
		/// The <see cref="ILoggerRepository"/> created will be associated with the repository
		/// specified such that a call to <see cref="GetRepository(Assembly)"/> with the
		/// same assembly specified will return the same repository instance.
		/// </para>
		/// </remarks>
		/// <param name="repositoryAssembly">The assembly to use to get the name of the repository.</param>
		/// <param name="repositoryType">A <see cref="Type"/> that implements <see cref="ILoggerRepository"/>
		/// and has a no arg constructor. An instance of this type will be created to act
		/// as the <see cref="ILoggerRepository"/> for the repository specified.</param>
		/// <returns>The <see cref="ILoggerRepository"/> created for the repository.</returns>
		[Obsolete("Use CreateRepository instead of CreateDomain")]
		public static ILoggerRepository CreateDomain(Assembly repositoryAssembly, Type repositoryType)
		{
			return LoggerManager.CreateRepository(repositoryAssembly, repositoryType);
		}

		/// <summary>
		/// Creates a repository for the specified assembly and repository type.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The <see cref="ILoggerRepository"/> created will be associated with the repository
		/// specified such that a call to <see cref="GetRepository(Assembly)"/> with the
		/// same assembly specified will return the same repository instance.
		/// </para>
		/// </remarks>
		/// <param name="repositoryAssembly">The assembly to use to get the name of the repository.</param>
		/// <param name="repositoryType">A <see cref="Type"/> that implements <see cref="ILoggerRepository"/>
		/// and has a no arg constructor. An instance of this type will be created to act
		/// as the <see cref="ILoggerRepository"/> for the repository specified.</param>
		/// <returns>The <see cref="ILoggerRepository"/> created for the repository.</returns>
		public static ILoggerRepository CreateRepository(Assembly repositoryAssembly, Type repositoryType)
		{
			return LoggerManager.CreateRepository(repositoryAssembly, repositoryType);
		}

		/// <summary>
		/// Gets the list of currently defined repositories.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Get an array of all the <see cref="ILoggerRepository"/> objects that have been created.
		/// </para>
		/// </remarks>
		/// <returns>An array of all the known <see cref="ILoggerRepository"/> objects.</returns>
		public static ILoggerRepository[] GetAllRepositories()
		{
			return LoggerManager.GetAllRepositories();
		}

		#endregion Domain & Repository Manager Methods

		#region Extension Handlers

		/// <summary>
		/// Looks up the wrapper object for the logger specified.
		/// </summary>
		/// <param name="logger">The logger to get the wrapper for.</param>
		/// <returns>The wrapper for the logger specified.</returns>
		private static ILog WrapLogger(ILogger logger)
		{
			return (ILog)s_wrapperMap.GetWrapper(logger);
		}

		/// <summary>
		/// Looks up the wrapper objects for the loggers specified.
		/// </summary>
		/// <param name="loggers">The loggers to get the wrappers for.</param>
		/// <returns>The wrapper objects for the loggers specified.</returns>
		private static ILog[] WrapLoggers(ILogger[] loggers)
		{
			ILog[] results = new ILog[loggers.Length];
			for(int i=0; i<loggers.Length; i++)
			{
				results[i] = WrapLogger(loggers[i]);
			}
			return results;
		}

		/// <summary>
		/// Create the <see cref="ILoggerWrapper"/> objects used by
		/// this manager.
		/// </summary>
		/// <param name="logger">The logger to wrap.</param>
		/// <returns>The wrapper for the logger specified.</returns>
		private static ILoggerWrapper WrapperCreationHandler(ILogger logger)
		{
			return new LogImpl(logger);
		}

		#endregion

		#region Private Static Fields

		/// <summary>
		/// The wrapper map to use to hold the <see cref="LogImpl"/> objects.
		/// </summary>
		private static readonly WrapperMap s_wrapperMap = new WrapperMap(new WrapperCreationHandler(WrapperCreationHandler));

		#endregion Private Static Fields
	}
}
