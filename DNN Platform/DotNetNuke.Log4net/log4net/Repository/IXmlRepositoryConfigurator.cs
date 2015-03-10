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

namespace log4net.Repository
{
	/// <summary>
	/// Configure repository using XML
	/// </summary>
	/// <remarks>
	/// <para>
	/// Interface used by Xml configurator to configure a <see cref="ILoggerRepository"/>.
	/// </para>
	/// <para>
	/// A <see cref="ILoggerRepository"/> should implement this interface to support
	/// configuration by the <see cref="log4net.Config.XmlConfigurator"/>.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public interface IXmlRepositoryConfigurator
	{
		/// <summary>
		/// Initialize the repository using the specified config
		/// </summary>
		/// <param name="element">the element containing the root of the config</param>
		/// <remarks>
		/// <para>
		/// The schema for the XML configuration data is defined by
		/// the implementation.
		/// </para>
		/// </remarks>
		void Configure(System.Xml.XmlElement element);
	}
}
