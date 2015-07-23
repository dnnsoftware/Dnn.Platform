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

namespace log4net.Core
{
	/// <summary>
	/// A SecurityContext used by log4net when interacting with protected resources
	/// </summary>
	/// <remarks>
	/// <para>
	/// A SecurityContext used by log4net when interacting with protected resources
	/// for example with operating system services. This can be used to impersonate
	/// a principal that has been granted privileges on the system resources.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	public abstract class SecurityContext
	{
		/// <summary>
		/// Impersonate this SecurityContext
		/// </summary>
		/// <param name="state">State supplied by the caller</param>
		/// <returns>An <see cref="IDisposable"/> instance that will
		/// revoke the impersonation of this SecurityContext, or <c>null</c></returns>
		/// <remarks>
		/// <para>
		/// Impersonate this security context. Further calls on the current
		/// thread should now be made in the security context provided
		/// by this object. When the <see cref="IDisposable"/> result 
		/// <see cref="IDisposable.Dispose"/> method is called the security
		/// context of the thread should be reverted to the state it was in
		/// before <see cref="Impersonate"/> was called.
		/// </para>
		/// </remarks>
		public abstract IDisposable Impersonate(object state);
	}
}
