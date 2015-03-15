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

namespace log4net.Core
{
	/// <summary>
	/// Implementation of the <see cref="ILoggerWrapper"/> interface.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This class should be used as the base for all wrapper implementations.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public abstract class LoggerWrapperImpl : ILoggerWrapper
	{
		#region Protected Instance Constructors

		/// <summary>
		/// Constructs a new wrapper for the specified logger.
		/// </summary>
		/// <param name="logger">The logger to wrap.</param>
		/// <remarks>
		/// <para>
		/// Constructs a new wrapper for the specified logger.
		/// </para>
		/// </remarks>
		protected LoggerWrapperImpl(ILogger logger) 
		{
			m_logger = logger;
		}

		#endregion Public Instance Constructors

		#region Implementation of ILoggerWrapper

		/// <summary>
		/// Gets the implementation behind this wrapper object.
		/// </summary>
		/// <value>
		/// The <see cref="ILogger"/> object that this object is implementing.
		/// </value>
		/// <remarks>
		/// <para>
		/// The <c>Logger</c> object may not be the same object as this object 
		/// because of logger decorators.
		/// </para>
		/// <para>
		/// This gets the actual underlying objects that is used to process
		/// the log events.
		/// </para>
		/// </remarks>
		virtual public ILogger Logger
		{
			get { return m_logger; }
		}

		#endregion

		#region Private Instance Fields

		/// <summary>
		/// The logger that this object is wrapping
		/// </summary>
		private readonly ILogger m_logger;  
 
		#endregion Private Instance Fields
	}
}
