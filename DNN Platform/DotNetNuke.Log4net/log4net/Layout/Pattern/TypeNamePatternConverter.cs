﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace log4net.Layout.Pattern
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

    /// <summary>
    /// Pattern converter for the class name.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Outputs the <see cref="LocationInfo.ClassName"/> of the event.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell.</author>
    internal sealed class TypeNamePatternConverter : NamedPatternConverter
    {
        /// <summary>
        /// Gets the fully qualified name of the class.
        /// </summary>
        /// <param name="loggingEvent">the event being logged.</param>
        /// <returns>The fully qualified type name for the caller location.</returns>
        /// <remarks>
        /// <para>
        /// Returns the <see cref="LocationInfo.ClassName"/> of the <paramref name="loggingEvent"/>.
        /// </para>
        /// </remarks>
        protected override string GetFullyQualifiedName(LoggingEvent loggingEvent)
        {
            return loggingEvent.LocationInformation.ClassName;
        }
    }
}
