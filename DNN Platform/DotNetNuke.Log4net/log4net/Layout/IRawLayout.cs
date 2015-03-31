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

using log4net;
using log4net.Core;
using log4net.Util.TypeConverters;

namespace log4net.Layout
{
	/// <summary>
	/// Interface for raw layout objects
	/// </summary>
	/// <remarks>
	/// <para>
	/// Interface used to format a <see cref="LoggingEvent"/>
	/// to an object.
	/// </para>
	/// <para>
	/// This interface should not be confused with the
	/// <see cref="ILayout"/> interface. This interface is used in
	/// only certain specialized situations where a raw object is
	/// required rather than a formatted string. The <see cref="ILayout"/>
	/// is not generally useful than this interface.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	[TypeConverter(typeof(RawLayoutConverter))]
	public interface IRawLayout
	{
		/// <summary>
		/// Implement this method to create your own layout format.
		/// </summary>
		/// <param name="loggingEvent">The event to format</param>
		/// <returns>returns the formatted event</returns>
		/// <remarks>
		/// <para>
		/// Implement this method to create your own layout format.
		/// </para>
		/// </remarks>
		object Format(LoggingEvent loggingEvent);
	}
}
