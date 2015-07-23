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
using log4net.Util;

namespace log4net.Filter
{
	/// <summary>
	/// This is a very simple filter based on <see cref="Level"/> matching.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The filter admits two options <see cref="LevelToMatch"/> and
	/// <see cref="AcceptOnMatch"/>. If there is an exact match between the value
	/// of the <see cref="LevelToMatch"/> option and the <see cref="Level"/> of the 
	/// <see cref="LoggingEvent"/>, then the <see cref="Decide"/> method returns <see cref="FilterDecision.Accept"/> in 
	/// case the <see cref="AcceptOnMatch"/> option value is set
	/// to <c>true</c>, if it is <c>false</c> then 
	/// <see cref="FilterDecision.Deny"/> is returned. If the <see cref="Level"/> does not match then
	/// the result will be <see cref="FilterDecision.Neutral"/>.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public class LevelMatchFilter : FilterSkeleton
	{
		#region Member Variables

		/// <summary>
		/// flag to indicate if the filter should <see cref="FilterDecision.Accept"/> on a match
		/// </summary>
		private bool m_acceptOnMatch = true;

		/// <summary>
		/// the <see cref="Level"/> to match against
		/// </summary>
		private Level m_levelToMatch;

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		public LevelMatchFilter()
		{
		}

		#endregion

		/// <summary>
		/// <see cref="FilterDecision.Accept"/> when matching <see cref="LevelToMatch"/>
		/// </summary>
		/// <remarks>
		/// <para>
		/// The <see cref="AcceptOnMatch"/> property is a flag that determines
		/// the behavior when a matching <see cref="Level"/> is found. If the
		/// flag is set to true then the filter will <see cref="FilterDecision.Accept"/> the 
		/// logging event, otherwise it will <see cref="FilterDecision.Deny"/> the event.
		/// </para>
		/// <para>
		/// The default is <c>true</c> i.e. to <see cref="FilterDecision.Accept"/> the event.
		/// </para>
		/// </remarks>
		public bool AcceptOnMatch
		{
			get { return m_acceptOnMatch; }
			set { m_acceptOnMatch = value; }
		}

		/// <summary>
		/// The <see cref="Level"/> that the filter will match
		/// </summary>
		/// <remarks>
		/// <para>
		/// The level that this filter will attempt to match against the 
		/// <see cref="LoggingEvent"/> level. If a match is found then
		/// the result depends on the value of <see cref="AcceptOnMatch"/>.
		/// </para>
		/// </remarks>
		public Level LevelToMatch
		{
			get { return m_levelToMatch; }
			set { m_levelToMatch = value; }
		}

		#region Override implementation of FilterSkeleton

		/// <summary>
		/// Tests if the <see cref="Level"/> of the logging event matches that of the filter
		/// </summary>
		/// <param name="loggingEvent">the event to filter</param>
		/// <returns>see remarks</returns>
		/// <remarks>
		/// <para>
		/// If the <see cref="Level"/> of the event matches the level of the
		/// filter then the result of the function depends on the
		/// value of <see cref="AcceptOnMatch"/>. If it is true then
		/// the function will return <see cref="FilterDecision.Accept"/>, it it is false then it
		/// will return <see cref="FilterDecision.Deny"/>. If the <see cref="Level"/> does not match then
		/// the result will be <see cref="FilterDecision.Neutral"/>.
		/// </para>
		/// </remarks>
		override public FilterDecision Decide(LoggingEvent loggingEvent) 
		{
			if (loggingEvent == null)
			{
				throw new ArgumentNullException("loggingEvent");
			}

			if (m_levelToMatch != null && m_levelToMatch == loggingEvent.Level) 
			{
				// Found match
				return m_acceptOnMatch ? FilterDecision.Accept : FilterDecision.Deny;
			}
			return FilterDecision.Neutral;
		}

		#endregion
	}
}
