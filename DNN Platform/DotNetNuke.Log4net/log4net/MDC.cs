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

namespace log4net
{
	/// <summary>
	/// Implementation of Mapped Diagnostic Contexts.
	/// </summary>
	/// <remarks>
	/// <note>
	/// <para>
	/// The MDC is deprecated and has been replaced by the <see cref="ThreadContext.Properties"/>.
	/// The current MDC implementation forwards to the <c>ThreadContext.Properties</c>.
	/// </para>
	/// </note>
	/// <para>
	/// The MDC class is similar to the <see cref="NDC"/> class except that it is
	/// based on a map instead of a stack. It provides <i>mapped
	/// diagnostic contexts</i>. A <i>Mapped Diagnostic Context</i>, or
	/// MDC in short, is an instrument for distinguishing interleaved log
	/// output from different sources. Log output is typically interleaved
	/// when a server handles multiple clients near-simultaneously.
	/// </para>
	/// <para>
	/// The MDC is managed on a per thread basis.
	/// </para>
	/// </remarks>
	/// <threadsafety static="true" instance="true" />
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	/*[Obsolete("MDC has been replaced by ThreadContext.Properties")]*/
	public sealed class MDC
	{
		#region Private Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="MDC" /> class. 
		/// </summary>
		/// <remarks>
		/// Uses a private access modifier to prevent instantiation of this class.
		/// </remarks>
		private MDC()
		{
		}

		#endregion Private Instance Constructors

		#region Public Static Methods

		/// <summary>
		/// Gets the context value identified by the <paramref name="key" /> parameter.
		/// </summary>
		/// <param name="key">The key to lookup in the MDC.</param>
		/// <returns>The string value held for the key, or a <c>null</c> reference if no corresponding value is found.</returns>
		/// <remarks>
		/// <note>
		/// <para>
		/// The MDC is deprecated and has been replaced by the <see cref="ThreadContext.Properties"/>.
		/// The current MDC implementation forwards to the <c>ThreadContext.Properties</c>.
		/// </para>
		/// </note>
		/// <para>
		/// If the <paramref name="key" /> parameter does not look up to a
		/// previously defined context then <c>null</c> will be returned.
		/// </para>
		/// </remarks>
		/*[Obsolete("MDC has been replaced by ThreadContext.Properties")]*/
		public static string Get(string key)
		{
			object obj = ThreadContext.Properties[key];
			if (obj == null)
			{
				return null;
			}
			return obj.ToString();
		}

		/// <summary>
		/// Add an entry to the MDC
		/// </summary>
		/// <param name="key">The key to store the value under.</param>
		/// <param name="value">The value to store.</param>
		/// <remarks>
		/// <note>
		/// <para>
		/// The MDC is deprecated and has been replaced by the <see cref="ThreadContext.Properties"/>.
		/// The current MDC implementation forwards to the <c>ThreadContext.Properties</c>.
		/// </para>
		/// </note>
		/// <para>
		/// Puts a context value (the <paramref name="value" /> parameter) as identified
		/// with the <paramref name="key" /> parameter into the current thread's
		/// context map.
		/// </para>
		/// <para>
		/// If a value is already defined for the <paramref name="key" />
		/// specified then the value will be replaced. If the <paramref name="value" /> 
		/// is specified as <c>null</c> then the key value mapping will be removed.
		/// </para>
		/// </remarks>
		/*[Obsolete("MDC has been replaced by ThreadContext.Properties")]*/
		public static void Set(string key, string value)
		{
			ThreadContext.Properties[key] = value;
		}

		/// <summary>
		/// Removes the key value mapping for the key specified.
		/// </summary>
		/// <param name="key">The key to remove.</param>
		/// <remarks>
		/// <note>
		/// <para>
		/// The MDC is deprecated and has been replaced by the <see cref="ThreadContext.Properties"/>.
		/// The current MDC implementation forwards to the <c>ThreadContext.Properties</c>.
		/// </para>
		/// </note>
		/// <para>
		/// Remove the specified entry from this thread's MDC
		/// </para>
		/// </remarks>
		/*[Obsolete("MDC has been replaced by ThreadContext.Properties")]*/
		public static void Remove(string key)
		{
			ThreadContext.Properties.Remove(key);
		}

		/// <summary>
		/// Clear all entries in the MDC
		/// </summary>
		/// <remarks>
		/// <note>
		/// <para>
		/// The MDC is deprecated and has been replaced by the <see cref="ThreadContext.Properties"/>.
		/// The current MDC implementation forwards to the <c>ThreadContext.Properties</c>.
		/// </para>
		/// </note>
		/// <para>
		/// Remove all the entries from this thread's MDC
		/// </para>
		/// </remarks>
		/*[Obsolete("MDC has been replaced by ThreadContext.Properties")]*/
		public static void Clear()
		{
			ThreadContext.Properties.Clear();
		}

		#endregion Public Static Methods
	}
}
