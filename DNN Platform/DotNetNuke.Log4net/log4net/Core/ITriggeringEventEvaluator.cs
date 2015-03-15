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

namespace log4net.Core
{
	/// <summary>
	/// Test if an <see cref="LoggingEvent"/> triggers an action
	/// </summary>
	/// <remarks>
	/// <para>
	/// Implementations of this interface allow certain appenders to decide
	/// when to perform an appender specific action.
	/// </para>
	/// <para>
	/// The action or behavior triggered is defined by the implementation.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	public interface ITriggeringEventEvaluator
	{
		/// <summary>
		/// Test if this event triggers the action
		/// </summary>
		/// <param name="loggingEvent">The event to check</param>
		/// <returns><c>true</c> if this event triggers the action, otherwise <c>false</c></returns>
		/// <remarks>
		/// <para>
		/// Return <c>true</c> if this event triggers the action
		/// </para>
		/// </remarks>
		bool IsTriggeringEvent(LoggingEvent loggingEvent);
	}
}
