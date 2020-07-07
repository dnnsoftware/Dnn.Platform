// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace log4net.Util.PatternStringConverters
{
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
    using System.IO;
    using System.Text;

    using log4net.Core;
    using log4net.DateFormatter;
    using log4net.Util;

    /// <summary>
    /// Write the UTC date time to the output.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Date pattern converter, uses a <see cref="IDateFormatter"/> to format
    /// the current date and time in Universal time.
    /// </para>
    /// <para>
    /// See the <see cref="DatePatternConverter"/> for details on the date pattern syntax.
    /// </para>
    /// </remarks>
    /// <seealso cref="DatePatternConverter"/>
    /// <author>Nicko Cadell.</author>
    internal class UtcDatePatternConverter : DatePatternConverter
    {
        /// <summary>
        /// Write the current date and time to the output.
        /// </summary>
        /// <param name="writer"><see cref="TextWriter" /> that will receive the formatted result.</param>
        /// <param name="state">null, state is not set.</param>
        /// <remarks>
        /// <para>
        /// Pass the current date and time to the <see cref="IDateFormatter"/>
        /// for it to render it to the writer.
        /// </para>
        /// <para>
        /// The date is in Universal time when it is rendered.
        /// </para>
        /// </remarks>
        /// <seealso cref="DatePatternConverter"/>
        protected override void Convert(TextWriter writer, object state)
        {
            try
            {
                this.m_dateFormatter.FormatDate(DateTime.UtcNow, writer);
            }
            catch (Exception ex)
            {
                LogLog.Error(declaringType, "Error occurred while converting date.", ex);
            }
        }

        /// <summary>
        /// The fully qualified type of the UtcDatePatternConverter class.
        /// </summary>
        /// <remarks>
        /// Used by the internal logger to record the Type of the
        /// log message.
        /// </remarks>
        private static readonly Type declaringType = typeof(UtcDatePatternConverter);
    }
}
