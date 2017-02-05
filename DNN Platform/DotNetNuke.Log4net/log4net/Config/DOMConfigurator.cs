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
using System.Xml;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Threading;

using log4net.Appender;
using log4net.Util;
using log4net.Repository;

namespace log4net.Config
{
	/// <summary>
	/// Use this class to initialize the log4net environment using an Xml tree.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <b>DOMConfigurator is obsolete. Use XmlConfigurator instead of DOMConfigurator.</b>
	/// </para>
	/// <para>
	/// Configures a <see cref="ILoggerRepository"/> using an Xml tree.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	[Obsolete("Use XmlConfigurator instead of DOMConfigurator")]
	public sealed class DOMConfigurator
	{
		#region Private Instance Constructors

		/// <summary>
		/// Private constructor
		/// </summary>
		private DOMConfigurator() 
		{ 
		}

		#endregion Protected Instance Constructors

		#region Configure static methods

		/// <summary>
		/// Automatically configures the log4net system based on the 
		/// application's configuration settings.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <b>DOMConfigurator is obsolete. Use XmlConfigurator instead of DOMConfigurator.</b>
		/// </para>
		/// Each application has a configuration file. This has the
		/// same name as the application with '.config' appended.
		/// This file is XML and calling this function prompts the
		/// configurator to look in that file for a section called
		/// <c>log4net</c> that contains the configuration data.
		/// </remarks>
		[Obsolete("Use XmlConfigurator.Configure instead of DOMConfigurator.Configure")]
		static public void Configure() 
		{
			XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetCallingAssembly()));
		}

		/// <summary>
		/// Automatically configures the <see cref="ILoggerRepository"/> using settings
		/// stored in the application's configuration file.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <b>DOMConfigurator is obsolete. Use XmlConfigurator instead of DOMConfigurator.</b>
		/// </para>
		/// Each application has a configuration file. This has the
		/// same name as the application with '.config' appended.
		/// This file is XML and calling this function prompts the
		/// configurator to look in that file for a section called
		/// <c>log4net</c> that contains the configuration data.
		/// </remarks>
		/// <param name="repository">The repository to configure.</param>
		[Obsolete("Use XmlConfigurator.Configure instead of DOMConfigurator.Configure")]
		static public void Configure(ILoggerRepository repository) 
		{
			XmlConfigurator.Configure(repository);
		}

		/// <summary>
		/// Configures log4net using a <c>log4net</c> element
		/// </summary>
		/// <remarks>
		/// <para>
		/// <b>DOMConfigurator is obsolete. Use XmlConfigurator instead of DOMConfigurator.</b>
		/// </para>
		/// Loads the log4net configuration from the XML element
		/// supplied as <paramref name="element"/>.
		/// </remarks>
		/// <param name="element">The element to parse.</param>
		[Obsolete("Use XmlConfigurator.Configure instead of DOMConfigurator.Configure")]
		static public void Configure(XmlElement element) 
		{
			XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetCallingAssembly()), element);
		}

		/// <summary>
		/// Configures the <see cref="ILoggerRepository"/> using the specified XML 
		/// element.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <b>DOMConfigurator is obsolete. Use XmlConfigurator instead of DOMConfigurator.</b>
		/// </para>
		/// Loads the log4net configuration from the XML element
		/// supplied as <paramref name="element"/>.
		/// </remarks>
		/// <param name="repository">The repository to configure.</param>
		/// <param name="element">The element to parse.</param>
		[Obsolete("Use XmlConfigurator.Configure instead of DOMConfigurator.Configure")]
		static public void Configure(ILoggerRepository repository, XmlElement element) 
		{
			XmlConfigurator.Configure(repository, element);
		}

		/// <summary>
		/// Configures log4net using the specified configuration file.
		/// </summary>
		/// <param name="configFile">The XML file to load the configuration from.</param>
		/// <remarks>
		/// <para>
		/// <b>DOMConfigurator is obsolete. Use XmlConfigurator instead of DOMConfigurator.</b>
		/// </para>
		/// <para>
		/// The configuration file must be valid XML. It must contain
		/// at least one element called <c>log4net</c> that holds
		/// the log4net configuration data.
		/// </para>
		/// <para>
		/// The log4net configuration file can possible be specified in the application's
		/// configuration file (either <c>MyAppName.exe.config</c> for a
		/// normal application on <c>Web.config</c> for an ASP.NET application).
		/// </para>
		/// <example>
		/// The following example configures log4net using a configuration file, of which the 
		/// location is stored in the application's configuration file :
		/// </example>
		/// <code lang="C#">
		/// using log4net.Config;
		/// using System.IO;
		/// using System.Configuration;
		/// 
		/// ...
		/// 
		/// DOMConfigurator.Configure(new FileInfo(ConfigurationSettings.AppSettings["log4net-config-file"]));
		/// </code>
		/// <para>
		/// In the <c>.config</c> file, the path to the log4net can be specified like this :
		/// </para>
		/// <code lang="XML" escaped="true">
		/// <configuration>
		///		<appSettings>
		///			<add key="log4net-config-file" value="log.config"/>
		///		</appSettings>
		///	</configuration>
		/// </code>
		/// </remarks>
		[Obsolete("Use XmlConfigurator.Configure instead of DOMConfigurator.Configure")]
		static public void Configure(FileInfo configFile)
		{
			XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetCallingAssembly()), configFile);
		}

		/// <summary>
		/// Configures log4net using the specified configuration file.
		/// </summary>
		/// <param name="configStream">A stream to load the XML configuration from.</param>
		/// <remarks>
		/// <para>
		/// <b>DOMConfigurator is obsolete. Use XmlConfigurator instead of DOMConfigurator.</b>
		/// </para>
		/// <para>
		/// The configuration data must be valid XML. It must contain
		/// at least one element called <c>log4net</c> that holds
		/// the log4net configuration data.
		/// </para>
		/// <para>
		/// Note that this method will NOT close the stream parameter.
		/// </para>
		/// </remarks>
		[Obsolete("Use XmlConfigurator.Configure instead of DOMConfigurator.Configure")]
		static public void Configure(Stream configStream)
		{
			XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetCallingAssembly()), configStream);
		}

		/// <summary>
		/// Configures the <see cref="ILoggerRepository"/> using the specified configuration 
		/// file.
		/// </summary>
		/// <param name="repository">The repository to configure.</param>
		/// <param name="configFile">The XML file to load the configuration from.</param>
		/// <remarks>
		/// <para>
		/// <b>DOMConfigurator is obsolete. Use XmlConfigurator instead of DOMConfigurator.</b>
		/// </para>
		/// <para>
		/// The configuration file must be valid XML. It must contain
		/// at least one element called <c>log4net</c> that holds
		/// the configuration data.
		/// </para>
		/// <para>
		/// The log4net configuration file can possible be specified in the application's
		/// configuration file (either <c>MyAppName.exe.config</c> for a
		/// normal application on <c>Web.config</c> for an ASP.NET application).
		/// </para>
		/// <example>
		/// The following example configures log4net using a configuration file, of which the 
		/// location is stored in the application's configuration file :
		/// </example>
		/// <code lang="C#">
		/// using log4net.Config;
		/// using System.IO;
		/// using System.Configuration;
		/// 
		/// ...
		/// 
		/// DOMConfigurator.Configure(new FileInfo(ConfigurationSettings.AppSettings["log4net-config-file"]));
		/// </code>
		/// <para>
		/// In the <c>.config</c> file, the path to the log4net can be specified like this :
		/// </para>
		/// <code lang="XML" escaped="true">
		/// <configuration>
		///		<appSettings>
		///			<add key="log4net-config-file" value="log.config"/>
		///		</appSettings>
		///	</configuration>
		/// </code>
		/// </remarks>
		[Obsolete("Use XmlConfigurator.Configure instead of DOMConfigurator.Configure")]
		static public void Configure(ILoggerRepository repository, FileInfo configFile)
		{
			XmlConfigurator.Configure(repository, configFile);
		}


		/// <summary>
		/// Configures the <see cref="ILoggerRepository"/> using the specified configuration 
		/// file.
		/// </summary>
		/// <param name="repository">The repository to configure.</param>
		/// <param name="configStream">The stream to load the XML configuration from.</param>
		/// <remarks>
		/// <para>
		/// <b>DOMConfigurator is obsolete. Use XmlConfigurator instead of DOMConfigurator.</b>
		/// </para>
		/// <para>
		/// The configuration data must be valid XML. It must contain
		/// at least one element called <c>log4net</c> that holds
		/// the configuration data.
		/// </para>
		/// <para>
		/// Note that this method will NOT close the stream parameter.
		/// </para>
		/// </remarks>
		[Obsolete("Use XmlConfigurator.Configure instead of DOMConfigurator.Configure")]
		static public void Configure(ILoggerRepository repository, Stream configStream)
		{
			XmlConfigurator.Configure(repository, configStream);
		}

		#endregion Configure static methods

		#region ConfigureAndWatch static methods

#if (!NETCF && !SSCLI)

		/// <summary>
		/// Configures log4net using the file specified, monitors the file for changes 
		/// and reloads the configuration if a change is detected.
		/// </summary>
		/// <param name="configFile">The XML file to load the configuration from.</param>
		/// <remarks>
		/// <para>
		/// <b>DOMConfigurator is obsolete. Use XmlConfigurator instead of DOMConfigurator.</b>
		/// </para>
		/// <para>
		/// The configuration file must be valid XML. It must contain
		/// at least one element called <c>log4net</c> that holds
		/// the configuration data.
		/// </para>
		/// <para>
		/// The configuration file will be monitored using a <see cref="FileSystemWatcher"/>
		/// and depends on the behavior of that class.
		/// </para>
		/// <para>
		/// For more information on how to configure log4net using
		/// a separate configuration file, see <see cref="M:Configure(FileInfo)"/>.
		/// </para>
		/// </remarks>
		/// <seealso cref="M:Configure(FileInfo)"/>
		[Obsolete("Use XmlConfigurator.ConfigureAndWatch instead of DOMConfigurator.ConfigureAndWatch")]
		static public void ConfigureAndWatch(FileInfo configFile)
		{
			XmlConfigurator.ConfigureAndWatch(LogManager.GetRepository(Assembly.GetCallingAssembly()), configFile);
		}

		/// <summary>
		/// Configures the <see cref="ILoggerRepository"/> using the file specified, 
		/// monitors the file for changes and reloads the configuration if a change 
		/// is detected.
		/// </summary>
		/// <param name="repository">The repository to configure.</param>
		/// <param name="configFile">The XML file to load the configuration from.</param>
		/// <remarks>
		/// <para>
		/// <b>DOMConfigurator is obsolete. Use XmlConfigurator instead of DOMConfigurator.</b>
		/// </para>
		/// <para>
		/// The configuration file must be valid XML. It must contain
		/// at least one element called <c>log4net</c> that holds
		/// the configuration data.
		/// </para>
		/// <para>
		/// The configuration file will be monitored using a <see cref="FileSystemWatcher"/>
		/// and depends on the behavior of that class.
		/// </para>
		/// <para>
		/// For more information on how to configure log4net using
		/// a separate configuration file, see <see cref="M:Configure(FileInfo)"/>.
		/// </para>
		/// </remarks>
		/// <seealso cref="M:Configure(FileInfo)"/>
		[Obsolete("Use XmlConfigurator.ConfigureAndWatch instead of DOMConfigurator.ConfigureAndWatch")]
		static public void ConfigureAndWatch(ILoggerRepository repository, FileInfo configFile)
		{
			XmlConfigurator.ConfigureAndWatch(repository, configFile);
		}
#endif

		#endregion ConfigureAndWatch static methods
	}
}

