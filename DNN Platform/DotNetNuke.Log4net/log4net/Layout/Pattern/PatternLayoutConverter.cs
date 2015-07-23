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
using System.IO;
using System.Collections;

using log4net.Core;
using log4net.Util;
using log4net.Repository;

namespace log4net.Layout.Pattern
{
	/// <summary>
	/// Abstract class that provides the formatting functionality that 
	/// derived classes need.
	/// </summary>
	/// <remarks>
	/// Conversion specifiers in a conversion patterns are parsed to
	/// individual PatternConverters. Each of which is responsible for
	/// converting a logging event in a converter specific manner.
	/// </remarks>
	/// <author>Nicko Cadell</author>
	public abstract class PatternLayoutConverter : PatternConverter
	{
		#region Protected Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="PatternLayoutConverter" /> class.
		/// </summary>
		protected PatternLayoutConverter() 
		{  
		}

		#endregion Protected Instance Constructors

		#region Public Properties

		/// <summary>
		/// Flag indicating if this converter handles the logging event exception
		/// </summary>
		/// <value><c>false</c> if this converter handles the logging event exception</value>
		/// <remarks>
		/// <para>
		/// If this converter handles the exception object contained within
		/// <see cref="LoggingEvent"/>, then this property should be set to
		/// <c>false</c>. Otherwise, if the layout ignores the exception
		/// object, then the property should be set to <c>true</c>.
		/// </para>
		/// <para>
		/// Set this value to override a this default setting. The default
		/// value is <c>true</c>, this converter does not handle the exception.
		/// </para>
		/// </remarks>
		virtual public bool IgnoresException 
		{ 
			get { return m_ignoresException; }
			set { m_ignoresException = value; }
		}

		#endregion Public Properties

		#region Protected Abstract Methods

		/// <summary>
		/// Derived pattern converters must override this method in order to
		/// convert conversion specifiers in the correct way.
		/// </summary>
		/// <param name="writer"><see cref="TextWriter" /> that will receive the formatted result.</param>
		/// <param name="loggingEvent">The <see cref="LoggingEvent" /> on which the pattern converter should be executed.</param>
		abstract protected void Convert(TextWriter writer, LoggingEvent loggingEvent);

		#endregion Protected Abstract Methods

		#region Protected Methods

		/// <summary>
		/// Derived pattern converters must override this method in order to
		/// convert conversion specifiers in the correct way.
		/// </summary>
		/// <param name="writer"><see cref="TextWriter" /> that will receive the formatted result.</param>
		/// <param name="state">The state object on which the pattern converter should be executed.</param>
		override protected void Convert(TextWriter writer, object state)
		{
			LoggingEvent loggingEvent = state as LoggingEvent;
			if (loggingEvent != null)
			{
				Convert(writer, loggingEvent);
			}
			else
			{
				throw new ArgumentException("state must be of type ["+typeof(LoggingEvent).FullName+"]", "state");
			}
		}

		#endregion Protected Methods

		/// <summary>
		/// Flag indicating if this converter handles exceptions
		/// </summary>
		/// <remarks>
		/// <c>false</c> if this converter handles exceptions
		/// </remarks>
		private bool m_ignoresException = true;
	}
}
