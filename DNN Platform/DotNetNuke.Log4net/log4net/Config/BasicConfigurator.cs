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
using log4net.Layout;
using log4net.Util;
using log4net.Repository;
using log4net.Repository.Hierarchy;

namespace log4net.Config
{
	/// <summary>
	/// Use this class to quickly configure a <see cref="Hierarchy"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Allows very simple programmatic configuration of log4net.
	/// </para>
	/// <para>
	/// Only one appender can be configured using this configurator.
	/// The appender is set at the root of the hierarchy and all logging
	/// events will be delivered to that appender.
	/// </para>
	/// <para>
	/// Appenders can also implement the <see cref="log4net.Core.IOptionHandler"/> interface. Therefore
	/// they would require that the <see cref="M:log4net.Core.IOptionHandler.ActivateOptions()"/> method
	/// be called after the appenders properties have been configured.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public sealed class BasicConfigurator
    {
	    #region Private Static Fields

	    /// <summary>
	    /// The fully qualified type of the BasicConfigurator class.
	    /// </summary>
	    /// <remarks>
	    /// Used by the internal logger to record the Type of the
	    /// log message.
	    /// </remarks>
	    private readonly static Type declaringType = typeof(BasicConfigurator);

	    #endregion Private Static Fields

        #region Private Instance Constructors

        /// <summary>
		/// Initializes a new instance of the <see cref="BasicConfigurator" /> class. 
		/// </summary>
		/// <remarks>
		/// <para>
		/// Uses a private access modifier to prevent instantiation of this class.
		/// </para>
		/// </remarks>
		private BasicConfigurator()
		{
		}

		#endregion Private Instance Constructors

		#region Public Static Methods

#if !NETSTANDARD1_3
		/// <summary>
		/// Initializes the log4net system with a default configuration.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Initializes the log4net logging system using a <see cref="ConsoleAppender"/>
		/// that will write to <c>Console.Out</c>. The log messages are
		/// formatted using the <see cref="PatternLayout"/> layout object
		/// with the <see cref="PatternLayout.DetailConversionPattern"/>
		/// layout style.
		/// </para>
		/// </remarks>
        static public ICollection Configure()
		{
            return BasicConfigurator.Configure(LogManager.GetRepository(Assembly.GetCallingAssembly()));
        }

        /// <summary>
        /// Initializes the log4net system using the specified appenders.
        /// </summary>
        /// <param name="appenders">The appenders to use to log all logging events.</param>
        /// <remarks>
        /// <para>
        /// Initializes the log4net system using the specified appenders.
        /// </para>
        /// </remarks>
        static public ICollection Configure(params IAppender[] appenders)
        {
            ArrayList configurationMessages = new ArrayList();

            ILoggerRepository repository = LogManager.GetRepository(Assembly.GetCallingAssembly());

            using (new LogLog.LogReceivedAdapter(configurationMessages))
            {
                InternalConfigure(repository, appenders);
            }

            repository.ConfigurationMessages = configurationMessages;

            return configurationMessages;
        }

	    /// <summary>
		/// Initializes the log4net system using the specified appender.
		/// </summary>
		/// <param name="appender">The appender to use to log all logging events.</param>
		/// <remarks>
		/// <para>
		/// Initializes the log4net system using the specified appender.
		/// </para>
		/// </remarks>
		static public ICollection Configure(IAppender appender) 
		{
            return Configure(new IAppender[] { appender });
		}
#endif // !NETSTANDARD1_3

		/// <summary>
		/// Initializes the <see cref="ILoggerRepository"/> with a default configuration.
		/// </summary>
		/// <param name="repository">The repository to configure.</param>
		/// <remarks>
		/// <para>
		/// Initializes the specified repository using a <see cref="ConsoleAppender"/>
		/// that will write to <c>Console.Out</c>. The log messages are
		/// formatted using the <see cref="PatternLayout"/> layout object
		/// with the <see cref="PatternLayout.DetailConversionPattern"/>
		/// layout style.
		/// </para>
		/// </remarks>
        static public ICollection Configure(ILoggerRepository repository) 
		{
            ArrayList configurationMessages = new ArrayList();

            using (new LogLog.LogReceivedAdapter(configurationMessages))
            {
                // Create the layout
                PatternLayout layout = new PatternLayout();
                layout.ConversionPattern = PatternLayout.DetailConversionPattern;
                layout.ActivateOptions();

                // Create the appender
                ConsoleAppender appender = new ConsoleAppender();
                appender.Layout = layout;
                appender.ActivateOptions();

                InternalConfigure(repository, appender);
            }

            repository.ConfigurationMessages = configurationMessages;

            return configurationMessages;
		}

        /// <summary>
        /// Initializes the <see cref="ILoggerRepository"/> using the specified appender.
        /// </summary>
        /// <param name="repository">The repository to configure.</param>
        /// <param name="appender">The appender to use to log all logging events.</param>
        /// <remarks>
        /// <para>
        /// Initializes the <see cref="ILoggerRepository"/> using the specified appender.
        /// </para>
        /// </remarks>
        static public ICollection Configure(ILoggerRepository repository, IAppender appender)
        {
            return Configure(repository, new IAppender[] { appender });
        }

        /// <summary>
        /// Initializes the <see cref="ILoggerRepository"/> using the specified appenders.
        /// </summary>
        /// <param name="repository">The repository to configure.</param>
        /// <param name="appenders">The appenders to use to log all logging events.</param>
        /// <remarks>
        /// <para>
        /// Initializes the <see cref="ILoggerRepository"/> using the specified appender.
        /// </para>
        /// </remarks>
        static public ICollection Configure(ILoggerRepository repository, params IAppender[] appenders)
        {
            ArrayList configurationMessages = new ArrayList();

            using (new LogLog.LogReceivedAdapter(configurationMessages))
            {
                InternalConfigure(repository, appenders);
            }

            repository.ConfigurationMessages = configurationMessages;

            return configurationMessages;
        }
	    
		static private void InternalConfigure(ILoggerRepository repository, params IAppender[] appenders) 
		{
            IBasicRepositoryConfigurator configurableRepository = repository as IBasicRepositoryConfigurator;
            if (configurableRepository != null)
            {
                configurableRepository.Configure(appenders);
            }
            else
            {
                LogLog.Warn(declaringType, "BasicConfigurator: Repository [" + repository + "] does not support the BasicConfigurator");
            }
		}

		#endregion Public Static Methods
	}
}
