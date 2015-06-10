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

using log4net.Util;

namespace log4net.Util.PatternStringConverters
{
	/// <summary>
	/// Write the current process ID to the output
	/// </summary>
	/// <remarks>
	/// <para>
	/// Write the current process ID to the output writer
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	internal sealed class ProcessIdPatternConverter : PatternConverter 
	{
		/// <summary>
		/// Write the current process ID to the output
		/// </summary>
		/// <param name="writer">the writer to write to</param>
		/// <param name="state">null, state is not set</param>
		/// <remarks>
		/// <para>
		/// Write the current process ID to the output <paramref name="writer"/>.
		/// </para>
		/// </remarks>
#if NET_4_0
        [System.Security.SecuritySafeCritical]
#endif
        override protected void Convert(TextWriter writer, object state) 
		{
#if (NETCF || SSCLI)
			// On compact framework there is no System.Diagnostics.Process class
			writer.Write( SystemInfo.NotAvailableText );
#else
			try
			{
				writer.Write( System.Diagnostics.Process.GetCurrentProcess().Id );
			}
			catch(System.Security.SecurityException)
			{
				// This security exception will occur if the caller does not have 
				// some undefined set of SecurityPermission flags.
				LogLog.Debug(declaringType, "Security exception while trying to get current process id. Error Ignored.");

				writer.Write( SystemInfo.NotAvailableText );
			}
#endif
		}

	    #region Private Static Fields

	    /// <summary>
	    /// The fully qualified type of the ProcessIdPatternConverter class.
	    /// </summary>
	    /// <remarks>
	    /// Used by the internal logger to record the Type of the
	    /// log message.
	    /// </remarks>
	    private readonly static Type declaringType = typeof(ProcessIdPatternConverter);

	    #endregion Private Static Fields
	}
}
