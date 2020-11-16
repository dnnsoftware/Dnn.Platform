// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace log4net.Util
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

    using log4net.Util;

    /// <summary>
    /// Contain the information obtained when parsing formatting modifiers
    /// in conversion modifiers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Holds the formatting information extracted from the format string by
    /// the <see cref="PatternParser"/>. This is used by the <see cref="PatternConverter"/>
    /// objects when rendering the output.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell.</author>
    /// <author>Gert Driesen.</author>
    public class FormattingInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormattingInfo"/> class.
        /// Defaut Constructor.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Initializes a new instance of the <see cref="FormattingInfo" /> class.
        /// </para>
        /// </remarks>
        public FormattingInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormattingInfo"/> class.
        /// Constructor.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Initializes a new instance of the <see cref="FormattingInfo" /> class
        /// with the specified parameters.
        /// </para>
        /// </remarks>
        public FormattingInfo(int min, int max, bool leftAlign)
        {
            this.m_min = min;
            this.m_max = max;
            this.m_leftAlign = leftAlign;
        }

        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        /// <value>
        /// The minimum value.
        /// </value>
        /// <remarks>
        /// <para>
        /// Gets or sets the minimum value.
        /// </para>
        /// </remarks>
        public int Min
        {
            get { return this.m_min; }
            set { this.m_min = value; }
        }

        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        /// <value>
        /// The maximum value.
        /// </value>
        /// <remarks>
        /// <para>
        /// Gets or sets the maximum value.
        /// </para>
        /// </remarks>
        public int Max
        {
            get { return this.m_max; }
            set { this.m_max = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets a flag indicating whether left align is enabled
        /// or not.
        /// </summary>
        /// <value>
        /// A flag indicating whether left align is enabled or not.
        /// </value>
        /// <remarks>
        /// <para>
        /// Gets or sets a flag indicating whether left align is enabled or not.
        /// </para>
        /// </remarks>
        public bool LeftAlign
        {
            get { return this.m_leftAlign; }
            set { this.m_leftAlign = value; }
        }

        private int m_min = -1;
        private int m_max = int.MaxValue;
        private bool m_leftAlign = false;
    }
}
