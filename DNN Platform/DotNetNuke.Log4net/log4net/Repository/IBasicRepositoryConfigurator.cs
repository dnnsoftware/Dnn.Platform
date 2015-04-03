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

namespace log4net.Repository
{
	/// <summary>
	/// Basic Configurator interface for repositories
	/// </summary>
	/// <remarks>
	/// <para>
	/// Interface used by basic configurator to configure a <see cref="ILoggerRepository"/>
	/// with a default <see cref="log4net.Appender.IAppender"/>.
	/// </para>
	/// <para>
	/// A <see cref="ILoggerRepository"/> should implement this interface to support
	/// configuration by the <see cref="log4net.Config.BasicConfigurator"/>.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public interface IBasicRepositoryConfigurator
	{
		/// <summary>
		/// Initialize the repository using the specified appender
		/// </summary>
		/// <param name="appender">the appender to use to log all logging events</param>
		/// <remarks>
		/// <para>
		/// Configure the repository to route all logging events to the
		/// specified appender.
		/// </para>
		/// </remarks>
        void Configure(Appender.IAppender appender);

        /// <summary>
        /// Initialize the repository using the specified appenders
        /// </summary>
        /// <param name="appenders">the appenders to use to log all logging events</param>
        /// <remarks>
        /// <para>
        /// Configure the repository to route all logging events to the
        /// specified appenders.
        /// </para>
        /// </remarks>
        void Configure(params Appender.IAppender[] appenders);
	}
}
