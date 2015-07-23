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

// .NET Compact Framework 1.0 has no support for WindowsIdentity
#if !NETCF 
// MONO 1.0 has no support for Win32 Logon APIs
#if !MONO
// SSCLI 1.0 has no support for Win32 Logon APIs
#if !SSCLI
// We don't want framework or platform specific code in the CLI version of log4net
#if !CLI_1_0

using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.Permissions;

using log4net.Core;

namespace log4net.Util
{
	/// <summary>
	/// Impersonate a Windows Account
	/// </summary>
	/// <remarks>
	/// <para>
	/// This <see cref="SecurityContext"/> impersonates a Windows account.
	/// </para>
	/// <para>
	/// How the impersonation is done depends on the value of <see cref="Impersonate"/>.
	/// This allows the context to either impersonate a set of user credentials specified 
	/// using username, domain name and password or to revert to the process credentials.
	/// </para>
	/// </remarks>
	public class WindowsSecurityContext : SecurityContext, IOptionHandler
	{
		/// <summary>
		/// The impersonation modes for the <see cref="WindowsSecurityContext"/>
		/// </summary>
		/// <remarks>
		/// <para>
		/// See the <see cref="WindowsSecurityContext.Credentials"/> property for
		/// details.
		/// </para>
		/// </remarks>
		public enum ImpersonationMode
		{
			/// <summary>
			/// Impersonate a user using the credentials supplied
			/// </summary>
			User,

			/// <summary>
			/// Revert this the thread to the credentials of the process
			/// </summary>
			Process
		}

		#region Member Variables

		private ImpersonationMode m_impersonationMode = ImpersonationMode.User;
		private string m_userName;
		private string m_domainName = Environment.MachineName;
		private string m_password;
		private WindowsIdentity m_identity;

		#endregion

		#region Constructor

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <remarks>
		/// <para>
		/// Default constructor
		/// </para>
		/// </remarks>
		public WindowsSecurityContext()
		{
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the impersonation mode for this security context
		/// </summary>
		/// <value>
		/// The impersonation mode for this security context
		/// </value>
		/// <remarks>
		/// <para>
		/// Impersonate either a user with user credentials or
		/// revert this thread to the credentials of the process.
		/// The value is one of the <see cref="ImpersonationMode"/>
		/// enum.
		/// </para>
		/// <para>
		/// The default value is <see cref="ImpersonationMode.User"/>
		/// </para>
		/// <para>
		/// When the mode is set to <see cref="ImpersonationMode.User"/>
		/// the user's credentials are established using the
		/// <see cref="UserName"/>, <see cref="DomainName"/> and <see cref="Password"/>
		/// values.
		/// </para>
		/// <para>
		/// When the mode is set to <see cref="ImpersonationMode.Process"/>
		/// no other properties need to be set. If the calling thread is 
		/// impersonating then it will be reverted back to the process credentials.
		/// </para>
		/// </remarks>
		public ImpersonationMode Credentials
		{
			get { return m_impersonationMode; }
			set { m_impersonationMode = value; }
		}

		/// <summary>
		/// Gets or sets the Windows username for this security context
		/// </summary>
		/// <value>
		/// The Windows username for this security context
		/// </value>
		/// <remarks>
		/// <para>
		/// This property must be set if <see cref="Credentials"/>
		/// is set to <see cref="ImpersonationMode.User"/> (the default setting).
		/// </para>
		/// </remarks>
		public string UserName
		{
			get { return m_userName; }
			set { m_userName = value; }
		}

		/// <summary>
		/// Gets or sets the Windows domain name for this security context
		/// </summary>
		/// <value>
		/// The Windows domain name for this security context
		/// </value>
		/// <remarks>
		/// <para>
		/// The default value for <see cref="DomainName"/> is the local machine name
		/// taken from the <see cref="Environment.MachineName"/> property.
		/// </para>
		/// <para>
		/// This property must be set if <see cref="Credentials"/>
		/// is set to <see cref="ImpersonationMode.User"/> (the default setting).
		/// </para>
		/// </remarks>
		public string DomainName
		{
			get { return m_domainName; }
			set { m_domainName = value; }
		}

		/// <summary>
		/// Sets the password for the Windows account specified by the <see cref="UserName"/> and <see cref="DomainName"/> properties.
		/// </summary>
		/// <value>
		/// The password for the Windows account specified by the <see cref="UserName"/> and <see cref="DomainName"/> properties.
		/// </value>
		/// <remarks>
		/// <para>
		/// This property must be set if <see cref="Credentials"/>
		/// is set to <see cref="ImpersonationMode.User"/> (the default setting).
		/// </para>
		/// </remarks>
		public string Password
		{
			set { m_password = value; }
		}

		#endregion

		#region IOptionHandler Members

		/// <summary>
		/// Initialize the SecurityContext based on the options set.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This is part of the <see cref="IOptionHandler"/> delayed object
		/// activation scheme. The <see cref="ActivateOptions"/> method must 
		/// be called on this object after the configuration properties have
		/// been set. Until <see cref="ActivateOptions"/> is called this
		/// object is in an undefined state and must not be used. 
		/// </para>
		/// <para>
		/// If any of the configuration properties are modified then 
		/// <see cref="ActivateOptions"/> must be called again.
		/// </para>
		/// <para>
		/// The security context will try to Logon the specified user account and
		/// capture a primary token for impersonation.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">The required <see cref="UserName" />, 
		/// <see cref="DomainName" /> or <see cref="Password" /> properties were not specified.</exception>
		public void ActivateOptions()
		{
			if (m_impersonationMode == ImpersonationMode.User)
			{
				if (m_userName == null) throw new ArgumentNullException("m_userName");
				if (m_domainName == null) throw new ArgumentNullException("m_domainName");
				if (m_password == null) throw new ArgumentNullException("m_password");

				m_identity = LogonUser(m_userName, m_domainName, m_password);
			}
		}

		#endregion

		/// <summary>
		/// Impersonate the Windows account specified by the <see cref="UserName"/> and <see cref="DomainName"/> properties.
		/// </summary>
		/// <param name="state">caller provided state</param>
		/// <returns>
		/// An <see cref="IDisposable"/> instance that will revoke the impersonation of this SecurityContext
		/// </returns>
		/// <remarks>
		/// <para>
		/// Depending on the <see cref="Credentials"/> property either
		/// impersonate a user using credentials supplied or revert 
		/// to the process credentials.
		/// </para>
		/// </remarks>
		public override IDisposable Impersonate(object state)
		{
			if (m_impersonationMode == ImpersonationMode.User)
			{
				if (m_identity != null)
				{
					return new DisposableImpersonationContext(m_identity.Impersonate());
				}
			}
			else if (m_impersonationMode == ImpersonationMode.Process)
			{
				// Impersonate(0) will revert to the process credentials
				return new DisposableImpersonationContext(WindowsIdentity.Impersonate(IntPtr.Zero));
			}
			return null;
		}

		/// <summary>
		/// Create a <see cref="WindowsIdentity"/> given the userName, domainName and password.
		/// </summary>
		/// <param name="userName">the user name</param>
		/// <param name="domainName">the domain name</param>
		/// <param name="password">the password</param>
		/// <returns>the <see cref="WindowsIdentity"/> for the account specified</returns>
		/// <remarks>
		/// <para>
		/// Uses the Windows API call LogonUser to get a principal token for the account. This
		/// token is used to initialize the WindowsIdentity.
		/// </para>
		/// </remarks>
#if NET_4_0
        [System.Security.SecuritySafeCritical]
#endif
        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand, UnmanagedCode = true)]
        private static WindowsIdentity LogonUser(string userName, string domainName, string password)
		{
			const int LOGON32_PROVIDER_DEFAULT = 0;
			//This parameter causes LogonUser to create a primary token.
			const int LOGON32_LOGON_INTERACTIVE = 2;

			// Call LogonUser to obtain a handle to an access token.
			IntPtr tokenHandle = IntPtr.Zero;
			if(!LogonUser(userName, domainName, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref tokenHandle))
			{
				NativeError error = NativeError.GetLastError();
				throw new Exception("Failed to LogonUser ["+userName+"] in Domain ["+domainName+"]. Error: "+ error.ToString());
			}

			const int SecurityImpersonation = 2;
			IntPtr dupeTokenHandle = IntPtr.Zero;
			if(!DuplicateToken(tokenHandle, SecurityImpersonation, ref dupeTokenHandle))
			{
				NativeError error = NativeError.GetLastError();
				if (tokenHandle != IntPtr.Zero)
				{
					CloseHandle(tokenHandle);
				}
				throw new Exception("Failed to DuplicateToken after LogonUser. Error: " + error.ToString());
			}

			WindowsIdentity identity = new WindowsIdentity(dupeTokenHandle);

			// Free the tokens.
			if (dupeTokenHandle != IntPtr.Zero) 
			{
				CloseHandle(dupeTokenHandle);
			}
			if (tokenHandle != IntPtr.Zero)
			{
				CloseHandle(tokenHandle);
			}

			return identity;
		}

		#region Native Method Stubs

		[DllImport("advapi32.dll", SetLastError=true)]
		private static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

		[DllImport("kernel32.dll", CharSet=CharSet.Auto)]
		private extern static bool CloseHandle(IntPtr handle);

		[DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
		private extern static bool DuplicateToken(IntPtr ExistingTokenHandle, int SECURITY_IMPERSONATION_LEVEL, ref IntPtr DuplicateTokenHandle);

		#endregion

		#region DisposableImpersonationContext class

		/// <summary>
		/// Adds <see cref="IDisposable"/> to <see cref="WindowsImpersonationContext"/>
		/// </summary>
		/// <remarks>
		/// <para>
		/// Helper class to expose the <see cref="WindowsImpersonationContext"/>
		/// through the <see cref="IDisposable"/> interface.
		/// </para>
		/// </remarks>
		private sealed class DisposableImpersonationContext : IDisposable
		{
			private readonly WindowsImpersonationContext m_impersonationContext;

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="impersonationContext">the impersonation context being wrapped</param>
			/// <remarks>
			/// <para>
			/// Constructor
			/// </para>
			/// </remarks>
			public DisposableImpersonationContext(WindowsImpersonationContext impersonationContext)
			{
				m_impersonationContext = impersonationContext;
			}

			/// <summary>
			/// Revert the impersonation
			/// </summary>
			/// <remarks>
			/// <para>
			/// Revert the impersonation
			/// </para>
			/// </remarks>
			public void Dispose()
			{
				m_impersonationContext.Undo();
			}
		}

		#endregion
	}
}

#endif // !CLI_1_0
#endif // !SSCLI
#endif // !MONO
#endif // !NETCF

