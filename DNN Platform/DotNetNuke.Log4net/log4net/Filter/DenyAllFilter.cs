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

using log4net.Core;

namespace log4net.Filter
{
	/// <summary>
	/// This filter drops all <see cref="LoggingEvent"/>. 
	/// </summary>
	/// <remarks>
	/// <para>
	/// You can add this filter to the end of a filter chain to
	/// switch from the default "accept all unless instructed otherwise"
	/// filtering behavior to a "deny all unless instructed otherwise"
	/// behavior.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public sealed class DenyAllFilter : FilterSkeleton
	{
		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		public DenyAllFilter()
		{
		}

		#endregion

		#region Override implementation of FilterSkeleton

		/// <summary>
		/// Always returns the integer constant <see cref="FilterDecision.Deny"/>
		/// </summary>
		/// <param name="loggingEvent">the LoggingEvent to filter</param>
		/// <returns>Always returns <see cref="FilterDecision.Deny"/></returns>
		/// <remarks>
		/// <para>
		/// Ignores the event being logged and just returns
		/// <see cref="FilterDecision.Deny"/>. This can be used to change the default filter
		/// chain behavior from <see cref="FilterDecision.Accept"/> to <see cref="FilterDecision.Deny"/>. This filter
		/// should only be used as the last filter in the chain
		/// as any further filters will be ignored!
		/// </para>
		/// </remarks>
		override public FilterDecision Decide(LoggingEvent loggingEvent) 
		{
			return FilterDecision.Deny;
		}

		#endregion
	}
}
