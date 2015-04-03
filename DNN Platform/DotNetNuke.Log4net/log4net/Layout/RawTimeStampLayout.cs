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
using System.Text;

using log4net.Core;
using log4net.Util;

namespace log4net.Layout
{
	/// <summary>
	/// Extract the date from the <see cref="LoggingEvent"/>
	/// </summary>
	/// <remarks>
	/// <para>
	/// Extract the date from the <see cref="LoggingEvent"/>
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public class RawTimeStampLayout : IRawLayout
	{
		#region Constructors

		/// <summary>
		/// Constructs a RawTimeStampLayout
		/// </summary>
		public RawTimeStampLayout()
		{
		}

		#endregion
  
		#region Implementation of IRawLayout

		/// <summary>
		/// Gets the <see cref="LoggingEvent.TimeStamp"/> as a <see cref="DateTime"/>.
		/// </summary>
		/// <param name="loggingEvent">The event to format</param>
		/// <returns>returns the time stamp</returns>
		/// <remarks>
		/// <para>
		/// Gets the <see cref="LoggingEvent.TimeStamp"/> as a <see cref="DateTime"/>.
		/// </para>
		/// <para>
		/// The time stamp is in local time. To format the time stamp
		/// in universal time use <see cref="RawUtcTimeStampLayout"/>.
		/// </para>
		/// </remarks>
		public virtual object Format(LoggingEvent loggingEvent)
		{
			return loggingEvent.TimeStamp;
		}

		#endregion
	}
}
