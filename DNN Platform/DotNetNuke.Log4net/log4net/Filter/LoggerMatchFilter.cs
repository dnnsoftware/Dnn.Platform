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
	/// Simple filter to match a string in the event's logger name.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The works very similar to the <see cref="LevelMatchFilter"/>. It admits two 
	/// options <see cref="LoggerToMatch"/> and <see cref="AcceptOnMatch"/>. If the 
	/// <see cref="LoggingEvent.LoggerName"/> of the <see cref="LoggingEvent"/> starts 
	/// with the value of the <see cref="LoggerToMatch"/> option, then the 
	/// <see cref="Decide"/> method returns <see cref="FilterDecision.Accept"/> in 
	/// case the <see cref="AcceptOnMatch"/> option value is set to <c>true</c>, 
	/// if it is <c>false</c> then <see cref="FilterDecision.Deny"/> is returned.
	/// </para>
	/// </remarks>
	/// <author>Daniel Cazzulino</author>
	public class LoggerMatchFilter : FilterSkeleton
	{
		#region Member Variables

		/// <summary>
		/// Flag to indicate the behavior when we have a match
		/// </summary>
		private bool m_acceptOnMatch = true;

		/// <summary>
		/// The logger name string to substring match against the event
		/// </summary>
		private string m_loggerToMatch;

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		public LoggerMatchFilter()
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// <see cref="FilterDecision.Accept"/> when matching <see cref="LoggerToMatch"/>
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
		/// The <see cref="LoggingEvent.LoggerName"/> that the filter will match
		/// </summary>
		/// <remarks>
		/// <para>
		/// This filter will attempt to match this value against logger name in
		/// the following way. The match will be done against the beginning of the
		/// logger name (using <see cref="M:String.StartsWith(string)"/>). The match is
		/// case sensitive. If a match is found then
		/// the result depends on the value of <see cref="AcceptOnMatch"/>.
		/// </para>
		/// </remarks>
		public string LoggerToMatch
		{
			get { return m_loggerToMatch; }
			set { m_loggerToMatch = value; }
		}

		#endregion 

		#region Override implementation of FilterSkeleton

		/// <summary>
		/// Check if this filter should allow the event to be logged
		/// </summary>
		/// <param name="loggingEvent">the event being logged</param>
		/// <returns>see remarks</returns>
		/// <remarks>
		/// <para>
		/// The rendered message is matched against the <see cref="LoggerToMatch"/>.
		/// If the <see cref="LoggerToMatch"/> equals the beginning of 
		/// the incoming <see cref="LoggingEvent.LoggerName"/> (<see cref="M:String.StartsWith(string)"/>)
		/// then a match will have occurred. If no match occurs
		/// this function will return <see cref="FilterDecision.Neutral"/>
		/// allowing other filters to check the event. If a match occurs then
		/// the value of <see cref="AcceptOnMatch"/> is checked. If it is
		/// true then <see cref="FilterDecision.Accept"/> is returned otherwise
		/// <see cref="FilterDecision.Deny"/> is returned.
		/// </para>
		/// </remarks>
		override public FilterDecision Decide(LoggingEvent loggingEvent) 
		{
			if (loggingEvent == null)
			{
				throw new ArgumentNullException("loggingEvent");
			}

			// Check if we have been setup to filter
			if ((m_loggerToMatch != null && m_loggerToMatch.Length != 0) && 
				loggingEvent.LoggerName.StartsWith(m_loggerToMatch))
			{
				// we've got a match
				if (m_acceptOnMatch) 
				{
					return FilterDecision.Accept;
				} 
				return FilterDecision.Deny;
			}
			else
			{
				// We cannot filter so allow the filter chain
				// to continue processing
				return FilterDecision.Neutral;
			}
		}

		#endregion
	}
}
