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
using System.Text.RegularExpressions;

using log4net;
using log4net.Core;
using log4net.Util;

namespace log4net.Filter
{
	/// <summary>
	/// Simple filter to match a string an event property
	/// </summary>
	/// <remarks>
	/// <para>
	/// Simple filter to match a string in the value for a
	/// specific event property
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	public class PropertyFilter : StringMatchFilter
	{
		#region Member Variables

		/// <summary>
		/// The key to use to lookup the string from the event properties
		/// </summary>
		private string m_key;

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		public PropertyFilter()
		{
		}

		#endregion

		/// <summary>
		/// The key to lookup in the event properties and then match against.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The key name to use to lookup in the properties map of the
		/// <see cref="LoggingEvent"/>. The match will be performed against 
		/// the value of this property if it exists.
		/// </para>
		/// </remarks>
		public string Key
		{
			get { return m_key; }
			set { m_key = value; }
		}

		#region Override implementation of FilterSkeleton

		/// <summary>
		/// Check if this filter should allow the event to be logged
		/// </summary>
		/// <param name="loggingEvent">the event being logged</param>
		/// <returns>see remarks</returns>
		/// <remarks>
		/// <para>
		/// The event property for the <see cref="Key"/> is matched against 
		/// the <see cref="StringMatchFilter.StringToMatch"/>.
		/// If the <see cref="StringMatchFilter.StringToMatch"/> occurs as a substring within
		/// the property value then a match will have occurred. If no match occurs
		/// this function will return <see cref="FilterDecision.Neutral"/>
		/// allowing other filters to check the event. If a match occurs then
		/// the value of <see cref="StringMatchFilter.AcceptOnMatch"/> is checked. If it is
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

			// Check if we have a key to lookup the event property value with
			if (m_key == null)
			{
				// We cannot filter so allow the filter chain
				// to continue processing
				return FilterDecision.Neutral;
			}

			// Lookup the string to match in from the properties using 
			// the key specified.
			object msgObj = loggingEvent.LookupProperty(m_key);

			// Use an ObjectRenderer to convert the property value to a string
			string msg = loggingEvent.Repository.RendererMap.FindAndRender(msgObj);

			// Check if we have been setup to filter
			if (msg == null || (m_stringToMatch == null && m_regexToMatch == null))
			{
				// We cannot filter so allow the filter chain
				// to continue processing
				return FilterDecision.Neutral;
			}
    
			// Firstly check if we are matching using a regex
			if (m_regexToMatch != null)
			{
				// Check the regex
				if (m_regexToMatch.Match(msg).Success == false)
				{
					// No match, continue processing
					return FilterDecision.Neutral;
				} 

				// we've got a match
				if (m_acceptOnMatch) 
				{
					return FilterDecision.Accept;
				} 
				return FilterDecision.Deny;
			}
			else if (m_stringToMatch != null)
			{
				// Check substring match
				if (msg.IndexOf(m_stringToMatch) == -1) 
				{
					// No match, continue processing
					return FilterDecision.Neutral;
				} 

				// we've got a match
				if (m_acceptOnMatch) 
				{
					return FilterDecision.Accept;
				} 
				return FilterDecision.Deny;
			}
			return FilterDecision.Neutral;
		}

		#endregion
	}
}
