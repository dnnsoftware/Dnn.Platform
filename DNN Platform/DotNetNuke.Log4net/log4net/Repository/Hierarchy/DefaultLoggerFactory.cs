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

using log4net.Core;

namespace log4net.Repository.Hierarchy
{
	/// <summary>
	/// Default implementation of <see cref="ILoggerFactory"/>
	/// </summary>
	/// <remarks>
	/// <para>
	/// This default implementation of the <see cref="ILoggerFactory"/>
	/// interface is used to create the default subclass
	/// of the <see cref="Logger"/> object.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	internal class DefaultLoggerFactory : ILoggerFactory
	{
		#region Internal Instance Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="DefaultLoggerFactory" /> class. 
		/// </para>
		/// </remarks>
		internal DefaultLoggerFactory()
		{
		}

		#endregion Internal Instance Constructors

		#region Implementation of ILoggerFactory

		/// <summary>
		/// Create a new <see cref="Logger" /> instance
		/// </summary>
		/// <param name="repository">The <see cref="ILoggerRepository" /> that will own the <see cref="Logger" />.</param>
		/// <param name="name">The name of the <see cref="Logger" />.</param>
		/// <returns>The <see cref="Logger" /> instance for the specified name.</returns>
		/// <remarks>
		/// <para>
		/// Create a new <see cref="Logger" /> instance with the 
		/// specified name.
		/// </para>
		/// <para>
		/// Called by the <see cref="Hierarchy"/> to create
		/// new named <see cref="Logger"/> instances.
		/// </para>
		/// <para>
		/// If the <paramref name="name"/> is <c>null</c> then the root logger
		/// must be returned.
		/// </para>
		/// </remarks>
		public Logger CreateLogger(ILoggerRepository repository, string name) 
		{
			if (name == null)
			{
				return new RootLogger(repository.LevelMap.LookupWithDefault(Level.Debug));
			}
			return new LoggerImpl(name);
		}

		#endregion

		/// <summary>
		/// Default internal subclass of <see cref="Logger"/>
		/// </summary>
		/// <remarks>
		/// <para>
		/// This subclass has no additional behavior over the
		/// <see cref="Logger"/> class but does allow instances
		/// to be created.
		/// </para>
		/// </remarks>
		internal sealed class LoggerImpl : Logger
		{
			/// <summary>
			/// Construct a new Logger
			/// </summary>
			/// <param name="name">the name of the logger</param>
			/// <remarks>
			/// <para>
			/// Initializes a new instance of the <see cref="LoggerImpl" /> class
			/// with the specified name. 
			/// </para>
			/// </remarks>
			internal LoggerImpl(string name) : base(name)
			{
			}
		}
	}
}
