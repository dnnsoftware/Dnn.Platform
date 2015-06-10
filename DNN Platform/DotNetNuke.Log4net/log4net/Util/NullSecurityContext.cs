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

namespace log4net.Util
{
	/// <summary>
	/// A SecurityContext used when a SecurityContext is not required
	/// </summary>
	/// <remarks>
	/// <para>
	/// The <see cref="NullSecurityContext"/> is a no-op implementation of the
	/// <see cref="SecurityContext"/> base class. It is used where a <see cref="SecurityContext"/>
	/// is required but one has not been provided.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	public sealed class NullSecurityContext : SecurityContext
	{
		/// <summary>
		/// Singleton instance of <see cref="NullSecurityContext"/>
		/// </summary>
		/// <remarks>
		/// <para>
		/// Singleton instance of <see cref="NullSecurityContext"/>
		/// </para>
		/// </remarks>
		public static readonly NullSecurityContext Instance = new NullSecurityContext();

		/// <summary>
		/// Private constructor
		/// </summary>
		/// <remarks>
		/// <para>
		/// Private constructor for singleton pattern.
		/// </para>
		/// </remarks>
		private NullSecurityContext()
		{
		}

		/// <summary>
		/// Impersonate this SecurityContext
		/// </summary>
		/// <param name="state">State supplied by the caller</param>
		/// <returns><c>null</c></returns>
		/// <remarks>
		/// <para>
		/// No impersonation is done and <c>null</c> is always returned.
		/// </para>
		/// </remarks>
		public override IDisposable Impersonate(object state)
		{
			return null;
		}
	}
}
