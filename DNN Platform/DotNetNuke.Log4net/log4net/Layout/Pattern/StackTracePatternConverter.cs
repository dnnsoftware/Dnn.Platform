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
#if !NETCF
using System;
using System.IO;
using System.Diagnostics;

using log4net.Util;
using log4net.Core;

namespace log4net.Layout.Pattern
{
	/// <summary>
	/// Write the caller stack frames to the output
	/// </summary>
	/// <remarks>
	/// <para>
	/// Writes the <see cref="LocationInfo.StackFrames"/> to the output writer, using format:
	/// type3.MethodCall3 > type2.MethodCall2 > type1.MethodCall1
	/// </para>
	/// </remarks>
	/// <author>Michael Cromwell</author>
	internal class StackTracePatternConverter : PatternLayoutConverter, IOptionHandler
	{
		private int m_stackFrameLevel = 1;
		
		/// <summary>
		/// Initialize the converter
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
		/// </remarks>
		public void ActivateOptions()
		{
			if (Option == null)
				return;
			
			string optStr = Option.Trim();
			if (optStr.Length != 0)
			{
				int stackLevelVal;
				if (SystemInfo.TryParse(optStr, out stackLevelVal))
				{
					if (stackLevelVal <= 0) 
					{
						LogLog.Error(declaringType, "StackTracePatternConverter: StackeFrameLevel option (" + optStr + ") isn't a positive integer.");
					}
					else
					{
						m_stackFrameLevel = stackLevelVal;
					}
				} 
				else
				{
					LogLog.Error(declaringType, "StackTracePatternConverter: StackFrameLevel option \"" + optStr + "\" not a decimal integer.");
				}
			}
		}
		
		/// <summary>
		/// Write the strack frames to the output
		/// </summary>
		/// <param name="writer"><see cref="TextWriter" /> that will receive the formatted result.</param>
		/// <param name="loggingEvent">the event being logged</param>
		/// <remarks>
		/// <para>
		/// Writes the <see cref="LocationInfo.StackFrames"/> to the output writer.
		/// </para>
		/// </remarks>
		override protected void Convert(TextWriter writer, LoggingEvent loggingEvent)
		{
			StackFrameItem[] stackframes = loggingEvent.LocationInformation.StackFrames;
			if ((stackframes == null) || (stackframes.Length <= 0))
			{
				LogLog.Error(declaringType, "loggingEvent.LocationInformation.StackFrames was null or empty.");
				return;
			}
			
			int stackFrameIndex = m_stackFrameLevel - 1;
			while (stackFrameIndex >= 0)
			{
				if (stackFrameIndex >= stackframes.Length)
				{
					stackFrameIndex--;
					continue;
				}
				
				StackFrameItem stackFrame = stackframes[stackFrameIndex];
                writer.Write("{0}.{1}", stackFrame.ClassName, GetMethodInformation(stackFrame.Method));
				if (stackFrameIndex > 0)
				{
                    // TODO: make this user settable?
					writer.Write(" > ");
				}
				stackFrameIndex--;
			}
		}

                /// <summary>
        /// Returns the Name of the method
        /// </summary>
        /// <param name="method"></param>
        /// <remarks>This method was created, so this class could be used as a base class for StackTraceDetailPatternConverter</remarks>
        /// <returns>string</returns>
        internal virtual string GetMethodInformation(MethodItem method)
        {
            return method.Name;
        }
		
		#region Private Static Fields

	    /// <summary>
	    /// The fully qualified type of the StackTracePatternConverter class.
	    /// </summary>
	    /// <remarks>
	    /// Used by the internal logger to record the Type of the
	    /// log message.
	    /// </remarks>
	    private readonly static Type declaringType = typeof(StackTracePatternConverter);

	    #endregion Private Static Fields
	}
}
#endif
