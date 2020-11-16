// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#if !NETCF
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
using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;

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
        /// <summary>
        /// Initializes a new instance of the <see cref="StackFrameItem"/> class.
        /// returns a stack frame item from a stack frame. This.
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public StackFrameItem(StackFrame frame)
        {
            // set default values
            this.m_lineNumber = NA;
            this.m_fileName = NA;
            this.m_method = new MethodItem();
            this.m_className = NA;

            try
            {
                // get frame values
                this.m_lineNumber = frame.GetFileLineNumber().ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
                this.m_fileName = frame.GetFileName();

                // get method values
                MethodBase method = frame.GetMethod();
                if (method != null)
                {
                    if (method.DeclaringType != null)
                    {
                        this.m_className = method.DeclaringType.FullName;
                    }

                    this.m_method = new MethodItem(method);
                }
            }
            catch (Exception ex)
            {
                LogLog.Error(declaringType, "An exception ocurred while retreiving stack frame information.", ex);
            }

            // set full info
            this.m_fullInfo = this.m_className + '.' + this.m_method.Name + '(' + this.m_fileName + ':' + this.m_lineNumber + ')';
        }

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
            get { return this.m_className; }
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
            get { return this.m_fileName; }
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
            get { return this.m_lineNumber; }
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
            get { return this.m_method; }
        }

        /// <summary>
        /// Gets all available caller information.
        /// </summary>
        /// <value>
        /// All available caller information, in the format
        /// <c>fully.qualified.classname.of.caller.methodName(Filename:line)</c>.
        /// </value>
        /// <remarks>
        /// <para>
        /// Gets all available caller information, in the format
        /// <c>fully.qualified.classname.of.caller.methodName(Filename:line)</c>.
        /// </para>
        /// </remarks>
        public string FullInfo
        {
            get { return this.m_fullInfo; }
        }

        private readonly string m_lineNumber;
        private readonly string m_fileName;
        private readonly string m_className;
        private readonly string m_fullInfo;
        private readonly MethodItem m_method;

        /// <summary>
        /// The fully qualified type of the StackFrameItem class.
        /// </summary>
        /// <remarks>
        /// Used by the internal logger to record the Type of the
        /// log message.
        /// </remarks>
        private static readonly Type declaringType = typeof(StackFrameItem);

        /// <summary>
        /// When location information is not available the constant
        /// <c>NA</c> is returned. Current value of this string
        /// constant is <b>?</b>.
        /// </summary>
        private const string NA = "?";
    }
}
#endif
