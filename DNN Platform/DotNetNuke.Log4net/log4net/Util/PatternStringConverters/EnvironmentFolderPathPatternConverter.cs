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

namespace log4net.Util.PatternStringConverters
{
    /// <summary>
    /// Write an <see cref="System.Environment.SpecialFolder" /> folder path to the output
    /// </summary>
    /// <remarks>
    /// <para>
    /// Write an special path environment folder path to the output writer.
    /// The value of the <see cref="log4net.Util.PatternConverter.Option"/> determines 
    /// the name of the variable to output. <see cref="log4net.Util.PatternConverter.Option"/>
    /// should be a value in the <see cref="System.Environment.SpecialFolder" /> enumeration.
    /// </para>
    /// </remarks>
    /// <author>Ron Grabowski</author>
    internal sealed class EnvironmentFolderPathPatternConverter : PatternConverter
    {
        /// <summary>
        /// Write an special path environment folder path to the output
        /// </summary>
        /// <param name="writer">the writer to write to</param>
        /// <param name="state">null, state is not set</param>
        /// <remarks>
        /// <para>
        /// Writes the special path environment folder path to the output <paramref name="writer"/>.
        /// The name of the special path environment folder path to output must be set
        /// using the <see cref="log4net.Util.PatternConverter.Option"/>
        /// property.
        /// </para>
        /// </remarks>
        override protected void Convert(TextWriter writer, object state)
        {
            try
            {
                if (Option != null && Option.Length > 0)
                {
                    Environment.SpecialFolder specialFolder =
                        (Environment.SpecialFolder)Enum.Parse(typeof(Environment.SpecialFolder), Option, true);

                    string envFolderPathValue = Environment.GetFolderPath(specialFolder);
                    if (envFolderPathValue != null && envFolderPathValue.Length > 0)
                    {
                        writer.Write(envFolderPathValue);
                    }
                }
            }
            catch (System.Security.SecurityException secEx)
            {
                // This security exception will occur if the caller does not have 
                // unrestricted environment permission. If this occurs the expansion 
                // will be skipped with the following warning message.
                LogLog.Debug(declaringType, "Security exception while trying to expand environment variables. Error Ignored. No Expansion.", secEx);
            }
            catch (Exception ex)
            {
                LogLog.Error(declaringType, "Error occurred while converting environment variable.", ex);
            }
        }

        #region Private Static Fields

        /// <summary>
        /// The fully qualified type of the EnvironmentFolderPathPatternConverter class.
        /// </summary>
        /// <remarks>
        /// Used by the internal logger to record the Type of the
        /// log message.
        /// </remarks>
        private readonly static Type declaringType = typeof(EnvironmentFolderPathPatternConverter);

        #endregion Private Static Fields
    }
}

#endif // !NETCF