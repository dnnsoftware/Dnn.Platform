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

// MONO 1.0 has no support for Win32 OutputDebugString API
#if !MONO
// SSCLI 1.0 has no support for Win32 OutputDebugString API
#if !SSCLI
// We don't want framework or platform specific code in the CLI version of log4net
#if !CLI_1_0

using System.Runtime.InteropServices;

using log4net.Layout;
using log4net.Core;

namespace log4net.Appender
{
	/// <summary>
	/// Appends log events to the OutputDebugString system.
	/// </summary>
	/// <remarks>
	/// <para>
	/// OutputDebugStringAppender appends log events to the
	/// OutputDebugString system.
	/// </para>
	/// <para>
	/// The string is passed to the native <c>OutputDebugString</c> 
	/// function.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public class OutputDebugStringAppender : AppenderSkeleton
	{
		#region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="OutputDebugStringAppender" /> class.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Default constructor.
		/// </para>
		/// </remarks>
		public OutputDebugStringAppender()
		{
		}

		#endregion // Public Instance Constructors

		#region Override implementation of AppenderSkeleton

		/// <summary>
		/// Write the logging event to the output debug string API
		/// </summary>
		/// <param name="loggingEvent">the event to log</param>
		/// <remarks>
		/// <para>
		/// Write the logging event to the output debug string API
		/// </para>
		/// </remarks>
#if NET_4_0
        [System.Security.SecuritySafeCritical]
#elif !NETCF
        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand, UnmanagedCode = true)]
#endif
        override protected void Append(LoggingEvent loggingEvent) 
		{
			OutputDebugString(RenderLoggingEvent(loggingEvent));
		} 

		/// <summary>
		/// This appender requires a <see cref="Layout"/> to be set.
		/// </summary>
		/// <value><c>true</c></value>
		/// <remarks>
		/// <para>
		/// This appender requires a <see cref="Layout"/> to be set.
		/// </para>
		/// </remarks>
		override protected bool RequiresLayout
		{
			get { return true; }
		}

		#endregion // Override implementation of AppenderSkeleton

		#region Protected Static Methods

		/// <summary>
		/// Stub for OutputDebugString native method
		/// </summary>
		/// <param name="message">the string to output</param>
		/// <remarks>
		/// <para>
		/// Stub for OutputDebugString native method
		/// </para>
		/// </remarks>
#if NETCF
		[DllImport("CoreDll.dll")]
#else
		[DllImport("Kernel32.dll")]
#endif
		protected static extern void OutputDebugString(string message);

		#endregion // Protected Static Methods
	}
}

#endif // !CLI_1_0
#endif // !SSCLI
#endif // !MONO
