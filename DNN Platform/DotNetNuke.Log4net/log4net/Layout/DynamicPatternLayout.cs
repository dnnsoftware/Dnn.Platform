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
using System.Collections;
using System.IO;

using log4net.Core;
using log4net.Layout.Pattern;
using log4net.Util;

namespace log4net.Layout
{
	/// <summary>
	/// A flexible layout configurable with pattern string that re-evaluates on each call.
	/// </summary>
	/// <remarks>
	/// <para>This class is built on <see cref="PatternLayout"></see> and provides all the
	/// features and capabilities of PatternLayout.  PatternLayout is a 'static' class
	/// in that its layout is done once at configuration time.  This class will recreate
	/// the layout on each reference.</para>
	/// <para>One important difference between PatternLayout and DynamicPatternLayout is the
	/// treatment of the Header and Footer parameters in the configuration.  The Header and Footer
	/// parameters for DynamicPatternLayout must be syntactically in the form of a PatternString,
	/// but should not be marked as type log4net.Util.PatternString.  Doing so causes the
	/// pattern to be statically converted at configuration time and causes DynamicPatternLayout
	/// to perform the same as PatternLayout.</para>
	/// <para>Please see <see cref="PatternLayout"/> for complete documentation.</para>
	/// <example>
	///	&lt;layout type="log4net.Layout.DynamicPatternLayout"&gt;
	///   &lt;param name="Header" value="%newline**** Trace Opened     Local: %date{yyyy-MM-dd HH:mm:ss.fff}     UTC: %utcdate{yyyy-MM-dd HH:mm:ss.fff} ****%newline" /&gt;
	///   &lt;param name="Footer" value="**** Trace Closed %date{yyyy-MM-dd HH:mm:ss.fff} ****%newline" /&gt;
	/// &lt;/layout&gt;
	/// </example>
	/// </remarks>
	public class DynamicPatternLayout: PatternLayout
	{
		#region Member Variables
		/// <summary>
		/// The header PatternString
		/// </summary>
		private PatternString m_headerPatternString = new PatternString("");

		/// <summary>
		/// The footer PatternString
		/// </summary>
		private PatternString m_footerPatternString  = new PatternString("");
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a DynamicPatternLayout using the DefaultConversionPattern
		/// </summary>
		/// <remarks>
		/// <para>
		/// The default pattern just produces the application supplied message.
		/// </para>
		/// </remarks>
		public DynamicPatternLayout()
			: base()
		{
		}

		/// <summary>
		/// Constructs a DynamicPatternLayout using the supplied conversion pattern
		/// </summary>
		/// <param name="pattern">the pattern to use</param>
		/// <remarks>
		/// </remarks>
		public DynamicPatternLayout (string pattern) 
			: base(pattern)
		{
		}
		#endregion

		#region Override implementation of LayoutSkeleton
		/// <summary>
		/// The header for the layout format.
		/// </summary>
		/// <value>the layout header</value>
		/// <remarks>
		/// <para>
		/// The Header text will be appended before any logging events
		/// are formatted and appended.
		/// </para>
		/// The pattern will be formatted on each get operation.
		/// </remarks>
		public override string Header
		{
			get
			{
				return m_headerPatternString.Format();
			}
			set
			{
				base.Header = value;
				m_headerPatternString = new PatternString(value);
			}
		}		/* property DynamicPatternLayout Header */

		/// <summary>
		/// The footer for the layout format.
		/// </summary>
		/// <value>the layout footer</value>
		/// <remarks>
		/// <para>
		/// The Footer text will be appended after all the logging events
		/// have been formatted and appended.
		/// </para>
		/// The pattern will be formatted on each get operation.
		/// </remarks>
		public override string Footer
		{
			get
			{
				return m_footerPatternString.Format();
			}
			set
			{
				base.Footer = value;
				m_footerPatternString = new PatternString(value);
			}
		}		/* property DynamicPatternLayout Footer */
		#endregion
	}			/* class DynamicPatternLayout */
}				/* namespace log4net.Layout */
