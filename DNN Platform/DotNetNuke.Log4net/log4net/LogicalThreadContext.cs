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

// .NET Compact Framework 1.0 has no support for System.Runtime.Remoting.Messaging.CallContext
#if !NETCF

using System;
using System.Collections;

using log4net.Util;

namespace log4net
{
	/// <summary>
	/// The log4net Logical Thread Context.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The <c>LogicalThreadContext</c> provides a location for <see cref="System.Runtime.Remoting.Messaging.CallContext"/> specific debugging 
	/// information to be stored.
	/// The <c>LogicalThreadContext</c> properties override any <see cref="ThreadContext"/> or <see cref="GlobalContext"/>
	/// properties with the same name.
	/// </para>
	/// <para>
	/// The Logical Thread Context has a properties map and a stack.
	/// The properties and stack can 
	/// be included in the output of log messages. The <see cref="log4net.Layout.PatternLayout"/>
	/// supports selecting and outputting these properties.
	/// </para>
	/// <para>
	/// The Logical Thread Context provides a diagnostic context for the current call context. 
	/// This is an instrument for distinguishing interleaved log
	/// output from different sources. Log output is typically interleaved
	/// when a server handles multiple clients near-simultaneously.
	/// </para>
	/// <para>
	/// The Logical Thread Context is managed on a per <see cref="System.Runtime.Remoting.Messaging.CallContext"/> basis.
	/// </para>
	/// <para>
	/// The <see cref="System.Runtime.Remoting.Messaging.CallContext"/> requires a link time 
	/// <see cref="System.Security.Permissions.SecurityPermission"/> for the
	/// <see cref="System.Security.Permissions.SecurityPermissionFlag.Infrastructure"/>.
	/// If the calling code does not have this permission then this context will be disabled.
	/// It will not store any property values set on it.
	/// </para>
	/// </remarks>
	/// <example>Example of using the thread context properties to store a username.
	/// <code lang="C#">
	/// LogicalThreadContext.Properties["user"] = userName;
	///	log.Info("This log message has a LogicalThreadContext Property called 'user'");
	/// </code>
	/// </example>
	/// <example>Example of how to push a message into the context stack
	/// <code lang="C#">
	///	using(LogicalThreadContext.Stacks["LDC"].Push("my context message"))
	///	{
	///		log.Info("This log message has a LogicalThreadContext Stack message that includes 'my context message'");
	///	
	///	} // at the end of the using block the message is automatically popped 
	/// </code>
	/// </example>
	/// <threadsafety static="true" instance="true" />
	/// <author>Nicko Cadell</author>
	public sealed class LogicalThreadContext
	{
		#region Private Instance Constructors

		/// <summary>
		/// Private Constructor. 
		/// </summary>
		/// <remarks>
		/// <para>
		/// Uses a private access modifier to prevent instantiation of this class.
		/// </para>
		/// </remarks>
		private LogicalThreadContext()
		{
		}

		#endregion Private Instance Constructors

		#region Public Static Properties

		/// <summary>
		/// The thread properties map
		/// </summary>
		/// <value>
		/// The thread properties map
		/// </value>
		/// <remarks>
		/// <para>
		/// The <c>LogicalThreadContext</c> properties override any <see cref="ThreadContext"/> 
		/// or <see cref="GlobalContext"/> properties with the same name.
		/// </para>
		/// </remarks>
		public static LogicalThreadContextProperties Properties
		{
			get { return s_properties; }
		}

		/// <summary>
		/// The thread stacks
		/// </summary>
		/// <value>
		/// stack map
		/// </value>
		/// <remarks>
		/// <para>
		/// The logical thread stacks.
		/// </para>
		/// </remarks>
		public static ThreadContextStacks Stacks
		{
			get { return s_stacks; }
		}

		#endregion Public Static Properties

		#region Private Static Fields

		/// <summary>
		/// The thread context properties instance
		/// </summary>
		private readonly static LogicalThreadContextProperties s_properties = new LogicalThreadContextProperties();

		/// <summary>
		/// The thread context stacks instance
		/// </summary>
		private readonly static ThreadContextStacks s_stacks = new ThreadContextStacks(s_properties);

		#endregion Private Static Fields
	}
}

#endif