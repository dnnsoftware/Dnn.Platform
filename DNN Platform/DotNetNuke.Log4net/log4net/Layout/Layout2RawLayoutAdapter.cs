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
using System.IO;

using log4net;
using log4net.Core;

namespace log4net.Layout
{
	/// <summary>
	/// Adapts any <see cref="ILayout"/> to a <see cref="IRawLayout"/>
	/// </summary>
	/// <remarks>
	/// <para>
	/// Where an <see cref="IRawLayout"/> is required this adapter
	/// allows a <see cref="ILayout"/> to be specified.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public class Layout2RawLayoutAdapter : IRawLayout
	{
		#region Member Variables

		/// <summary>
		/// The layout to adapt
		/// </summary>
		private ILayout m_layout;

		#endregion

		#region Constructors

		/// <summary>
		/// Construct a new adapter
		/// </summary>
		/// <param name="layout">the layout to adapt</param>
		/// <remarks>
		/// <para>
		/// Create the adapter for the specified <paramref name="layout"/>.
		/// </para>
		/// </remarks>
		public Layout2RawLayoutAdapter(ILayout layout)
		{
			m_layout = layout;
		}

		#endregion

		#region Implementation of IRawLayout

		/// <summary>
		/// Format the logging event as an object.
		/// </summary>
		/// <param name="loggingEvent">The event to format</param>
		/// <returns>returns the formatted event</returns>
		/// <remarks>
		/// <para>
		/// Format the logging event as an object.
		/// </para>
		/// <para>
		/// Uses the <see cref="ILayout"/> object supplied to 
		/// the constructor to perform the formatting.
		/// </para>
		/// </remarks>
		virtual public object Format(LoggingEvent loggingEvent)
		{
			StringWriter writer = new StringWriter(System.Globalization.CultureInfo.InvariantCulture);
			m_layout.Format(writer, loggingEvent);
			return writer.ToString();
		}

		#endregion
	}
}
