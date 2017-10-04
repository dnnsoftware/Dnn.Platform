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
using System.Diagnostics;

using log4net.Util;

namespace log4net.Core
{
	/// <summary>
	/// The internal representation of caller location information.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This class uses the <c>System.Diagnostics.StackTrace</c> class to generate
	/// a call stack. The caller's information is then extracted from this stack.
	/// </para>
	/// <para>
	/// The <c>System.Diagnostics.StackTrace</c> class is not supported on the 
	/// .NET Compact Framework 1.0 therefore caller location information is not
	/// available on that framework.
	/// </para>
	/// <para>
	/// The <c>System.Diagnostics.StackTrace</c> class has this to say about Release builds:
	/// </para>
	/// <para>
	/// "StackTrace information will be most informative with Debug build configurations. 
	/// By default, Debug builds include debug symbols, while Release builds do not. The 
	/// debug symbols contain most of the file, method name, line number, and column 
	/// information used in constructing StackFrame and StackTrace objects. StackTrace 
	/// might not report as many method calls as expected, due to code transformations 
	/// that occur during optimization."
	/// </para>
	/// <para>
	/// This means that in a Release build the caller information may be incomplete or may 
	/// not exist at all! Therefore caller location information cannot be relied upon in a Release build.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
#if !NETCF
	[Serializable]
#endif
	public class LocationInfo
	{
		#region Public Instance Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="callerStackBoundaryDeclaringType">The declaring type of the method that is
		/// the stack boundary into the logging system for this call.</param>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="LocationInfo" />
		/// class based on the current thread.
		/// </para>
		/// </remarks>
		public LocationInfo(Type callerStackBoundaryDeclaringType) 
		{
			// Initialize all fields
			m_className = NA;
			m_fileName = NA;
			m_lineNumber = NA;
			m_methodName = NA;
			m_fullInfo = NA;

#if !(NETCF || NETSTANDARD1_3) // StackTrace isn't fully implemented for NETSTANDARD1_3 https://github.com/dotnet/corefx/issues/1797
			if (callerStackBoundaryDeclaringType != null)
			{
				try
				{
					StackTrace st = new StackTrace(true);
					int frameIndex = 0;
																				
					// skip frames not from fqnOfCallingClass
					while (frameIndex < st.FrameCount)
					{
						StackFrame frame = st.GetFrame(frameIndex);
						if (frame != null && frame.GetMethod().DeclaringType == callerStackBoundaryDeclaringType)
						{
							break;
						}
						frameIndex++;
					}

					// skip frames from fqnOfCallingClass
					while (frameIndex < st.FrameCount)
					{
						StackFrame frame = st.GetFrame(frameIndex);
						if (frame != null && frame.GetMethod().DeclaringType != callerStackBoundaryDeclaringType)
						{
							break;
						}
						frameIndex++;
					}

					if (frameIndex < st.FrameCount)
					{
						// take into account the frames we skip above
						int adjustedFrameCount = st.FrameCount - frameIndex;
                        ArrayList stackFramesList = new ArrayList(adjustedFrameCount);
						m_stackFrames = new StackFrameItem[adjustedFrameCount];
						for (int i=frameIndex; i < st.FrameCount; i++) 
						{
							stackFramesList.Add(new StackFrameItem(st.GetFrame(i)));
						}
												
						stackFramesList.CopyTo(m_stackFrames, 0);
						
						// now frameIndex is the first 'user' caller frame
						StackFrame locationFrame = st.GetFrame(frameIndex);

						if (locationFrame != null)
						{
							System.Reflection.MethodBase method = locationFrame.GetMethod();

							if (method != null)
							{
								m_methodName =  method.Name;
								if (method.DeclaringType != null)
								{
									m_className = method.DeclaringType.FullName;
								}
							}
							m_fileName = locationFrame.GetFileName();
							m_lineNumber = locationFrame.GetFileLineNumber().ToString(System.Globalization.NumberFormatInfo.InvariantInfo);

							// Combine all location info
							m_fullInfo =  m_className + '.' + m_methodName + '(' + m_fileName + ':' + m_lineNumber + ')';
						}
					}
				}
				catch(System.Security.SecurityException)
				{
					// This security exception will occur if the caller does not have 
					// some undefined set of SecurityPermission flags.
					LogLog.Debug(declaringType, "Security exception while trying to get caller stack frame. Error Ignored. Location Information Not Available.");
				}
			}
#endif
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="className">The fully qualified class name.</param>
		/// <param name="methodName">The method name.</param>
		/// <param name="fileName">The file name.</param>
		/// <param name="lineNumber">The line number of the method within the file.</param>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="LocationInfo" />
		/// class with the specified data.
		/// </para>
		/// </remarks>
		public LocationInfo(string className, string methodName, string fileName, string lineNumber)
		{
			m_className = className;
			m_fileName = fileName;
			m_lineNumber = lineNumber;
			m_methodName = methodName;
			m_fullInfo = m_className + '.' + m_methodName + '(' + m_fileName + 
				':' + m_lineNumber + ')';
		}

		#endregion Public Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Gets the fully qualified class name of the caller making the logging 
		/// request.
		/// </summary>
		/// <value>
		/// The fully qualified class name of the caller making the logging 
		/// request.
		/// </value>
		/// <remarks>
		/// <para>
		/// Gets the fully qualified class name of the caller making the logging 
		/// request.
		/// </para>
		/// </remarks>
		public string ClassName
		{
			get { return m_className; }
		}

		/// <summary>
		/// Gets the file name of the caller.
		/// </summary>
		/// <value>
		/// The file name of the caller.
		/// </value>
		/// <remarks>
		/// <para>
		/// Gets the file name of the caller.
		/// </para>
		/// </remarks>
		public string FileName
		{
			get { return m_fileName; }
		}

		/// <summary>
		/// Gets the line number of the caller.
		/// </summary>
		/// <value>
		/// The line number of the caller.
		/// </value>
		/// <remarks>
		/// <para>
		/// Gets the line number of the caller.
		/// </para>
		/// </remarks>
		public string LineNumber
		{
			get { return m_lineNumber; }
		}

		/// <summary>
		/// Gets the method name of the caller.
		/// </summary>
		/// <value>
		/// The method name of the caller.
		/// </value>
		/// <remarks>
		/// <para>
		/// Gets the method name of the caller.
		/// </para>
		/// </remarks>
		public string MethodName
		{
			get { return m_methodName; }
		}

		/// <summary>
		/// Gets all available caller information
		/// </summary>
		/// <value>
		/// All available caller information, in the format
		/// <c>fully.qualified.classname.of.caller.methodName(Filename:line)</c>
		/// </value>
		/// <remarks>
		/// <para>
		/// Gets all available caller information, in the format
		/// <c>fully.qualified.classname.of.caller.methodName(Filename:line)</c>
		/// </para>
		/// </remarks>
		public string FullInfo
		{
			get { return m_fullInfo; }
		}
		
#if !(NETCF || NETSTANDARD1_3)
		/// <summary>
		/// Gets the stack frames from the stack trace of the caller making the log request
		/// </summary>
		public StackFrameItem[] StackFrames
		{
			get { return m_stackFrames; }
		}
#endif

		#endregion Public Instance Properties

		#region Private Instance Fields

		private readonly string m_className;
		private readonly string m_fileName;
		private readonly string m_lineNumber;
		private readonly string m_methodName;
		private readonly string m_fullInfo;
#if !(NETCF || NETSTANDARD1_3)
		private readonly StackFrameItem[] m_stackFrames;
#endif

		#endregion Private Instance Fields

		#region Private Static Fields

	    /// <summary>
	    /// The fully qualified type of the LocationInfo class.
	    /// </summary>
	    /// <remarks>
	    /// Used by the internal logger to record the Type of the
	    /// log message.
	    /// </remarks>
	    private readonly static Type declaringType = typeof(LocationInfo);

		/// <summary>
		/// When location information is not available the constant
		/// <c>NA</c> is returned. Current value of this string
		/// constant is <b>?</b>.
		/// </summary>
		private const string NA = "?";

		#endregion Private Static Fields
	}
}
