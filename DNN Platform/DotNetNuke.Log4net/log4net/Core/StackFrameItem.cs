#if !NETCF
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
using System.Diagnostics;
using System.Reflection;
using log4net.Util;

namespace log4net.Core
{
    /// <summary>
    /// provides stack frame information without actually referencing a System.Diagnostics.StackFrame
    /// as that would require that the containing assembly is loaded.
    /// </summary>
    /// 
    [Serializable]
    public class StackFrameItem
    {
        #region Public Instance Constructors

        /// <summary>
        /// returns a stack frame item from a stack frame. This 
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public StackFrameItem(StackFrame frame)
        {
            // set default values
            m_lineNumber = NA;
            m_fileName = NA;
            m_method = new MethodItem();
            m_className = NA;

			try
			{
				// get frame values
				m_lineNumber = frame.GetFileLineNumber().ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
				m_fileName = frame.GetFileName();
				// get method values
				MethodBase method = frame.GetMethod();
				if (method != null)
				{
					if(method.DeclaringType != null)
						m_className = method.DeclaringType.FullName;
					m_method = new MethodItem(method);
				}
			}
			catch (Exception ex)
			{
				LogLog.Error(declaringType, "An exception ocurred while retreiving stack frame information.", ex);
			}

            // set full info
            m_fullInfo = m_className + '.' + m_method.Name + '(' + m_fileName + ':' + m_lineNumber + ')';
        }

        #endregion

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
        public MethodItem Method
        {
            get { return m_method; }
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

        #endregion Public Instance Properties

        #region Private Instance Fields

        private readonly string m_lineNumber;
        private readonly string m_fileName;
        private readonly string m_className;
        private readonly string m_fullInfo;
		private readonly MethodItem m_method;

        #endregion

        #region Private Static Fields

        /// <summary>
        /// The fully qualified type of the StackFrameItem class.
        /// </summary>
        /// <remarks>
        /// Used by the internal logger to record the Type of the
        /// log message.
        /// </remarks>
        private readonly static Type declaringType = typeof(StackFrameItem);

        /// <summary>
        /// When location information is not available the constant
        /// <c>NA</c> is returned. Current value of this string
        /// constant is <b>?</b>.
        /// </summary>
        private const string NA = "?";

        #endregion Private Static Fields
    }
}
#endif
