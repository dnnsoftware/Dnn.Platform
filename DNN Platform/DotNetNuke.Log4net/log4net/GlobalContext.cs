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

namespace log4net
{
	/// <summary>
	/// The log4net Global Context.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The <c>GlobalContext</c> provides a location for global debugging 
	/// information to be stored.
	/// </para>
	/// <para>
	/// The global context has a properties map and these properties can 
	/// be included in the output of log messages. The <see cref="log4net.Layout.PatternLayout"/>
	/// supports selecting and outputing these properties.
	/// </para>
	/// <para>
	/// By default the <c>log4net:HostName</c> property is set to the name of 
	/// the current machine.
	/// </para>
	/// </remarks>
	/// <example>
	/// <code lang="C#">
	/// GlobalContext.Properties["hostname"] = Environment.MachineName;
	/// </code>
	/// </example>
	/// <threadsafety static="true" instance="true" />
	/// <author>Nicko Cadell</author>
	public sealed class GlobalContext
	{
		#region Private Instance Constructors

		/// <summary>
		/// Private Constructor. 
		/// </summary>
		/// <remarks>
		/// Uses a private access modifier to prevent instantiation of this class.
		/// </remarks>
		private GlobalContext()
		{
		}

		#endregion Private Instance Constructors

		static GlobalContext()
		{
			Properties[log4net.Core.LoggingEvent.HostNameProperty] = SystemInfo.HostName;
		}

		#region Public Static Properties

		/// <summary>
		/// The global properties map.
		/// </summary>
		/// <value>
		/// The global properties map.
		/// </value>
		/// <remarks>
		/// <para>
		/// The global properties map.
		/// </para>
		/// </remarks>
		public static GlobalContextProperties Properties
		{
			get { return s_properties; }
		}

		#endregion Public Static Properties

		#region Private Static Fields

		/// <summary>
		/// The global context properties instance
		/// </summary>
		private readonly static GlobalContextProperties s_properties = new GlobalContextProperties();

		#endregion Private Static Fields
	}
}
