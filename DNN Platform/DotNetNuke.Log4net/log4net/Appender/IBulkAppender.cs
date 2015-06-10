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

namespace log4net.Appender
{
	/// <summary>
	/// Interface for appenders that support bulk logging.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This interface extends the <see cref="IAppender"/> interface to
	/// support bulk logging of <see cref="LoggingEvent"/> objects. Appenders
	/// should only implement this interface if they can bulk log efficiently.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	public interface IBulkAppender : IAppender
	{
		/// <summary>
		/// Log the array of logging events in Appender specific way.
		/// </summary>
		/// <param name="loggingEvents">The events to log</param>
		/// <remarks>
		/// <para>
		/// This method is called to log an array of events into this appender.
		/// </para>
		/// </remarks>
		void DoAppend(LoggingEvent[] loggingEvents);
	}
}
