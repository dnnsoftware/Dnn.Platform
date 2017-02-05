/*
 *
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 *
*/
#if !NETCF

using System;
using System.Collections;
using System.Text;

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
    /// type3.MethodCall3(type param,...) > type2.MethodCall2(type param,...) > type1.MethodCall1(type param,...)
    /// </para>
    /// </remarks>
    /// <author>Adam Davies</author>
    internal class StackTraceDetailPatternConverter : StackTracePatternConverter
    {
        internal override string GetMethodInformation(MethodItem method)
        {
            string returnValue="";

            try
            {
                string param = "";
                string[] names = method.Parameters;
                StringBuilder sb = new StringBuilder();
                if (names != null && names.GetUpperBound(0) > 0)
                {
                    for (int i = 0; i <= names.GetUpperBound(0); i++)
                    {
                        sb.AppendFormat("{0}, ", names[i]);
                    }
                }

                if (sb.Length > 0)
                {
                    sb.Remove(sb.Length - 2, 2);
                    param = sb.ToString();
                }

                returnValue=base.GetMethodInformation(method) + "(" + param + ")";
            }
            catch (Exception ex)
            {
                LogLog.Error(declaringType, "An exception ocurred while retreiving method information.", ex);
            }

            return returnValue;
        }

        #region Private Static Fields

        /// <summary>
        /// The fully qualified type of the StackTraceDetailPatternConverter class.
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