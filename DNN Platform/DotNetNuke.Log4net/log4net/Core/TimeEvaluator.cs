// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace log4net.Core
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

    /// <summary>
    /// An evaluator that triggers after specified number of seconds.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This evaluator will trigger if the specified time period
    /// <see cref="Interval"/> has passed since last check.
    /// </para>
    /// </remarks>
    /// <author>Robert Sevcik.</author>
    public class TimeEvaluator : ITriggeringEventEvaluator
    {
        /// <summary>
        /// The time threshold for triggering in seconds. Zero means it won't trigger at all.
        /// </summary>
        private int m_interval;

        /// <summary>
        /// The UTC time of last check. This gets updated when the object is created and when the evaluator triggers.
        /// </summary>
        private DateTime m_lastTimeUtc;

        /// <summary>
        /// The default time threshold for triggering in seconds. Zero means it won't trigger at all.
        /// </summary>
        private const int DEFAULT_INTERVAL = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeEvaluator"/> class.
        /// Create a new evaluator using the <see cref="DEFAULT_INTERVAL"/> time threshold in seconds.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Create a new evaluator using the <see cref="DEFAULT_INTERVAL"/> time threshold in seconds.
        /// </para>
        /// <para>
        /// This evaluator will trigger if the specified time period
        /// <see cref="Interval"/> has passed since last check.
        /// </para>
        /// </remarks>
        public TimeEvaluator()
            : this(DEFAULT_INTERVAL)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeEvaluator"/> class.
        /// Create a new evaluator using the specified time threshold in seconds.
        /// </summary>
        /// <param name="interval">
        /// The time threshold in seconds to trigger after.
        /// Zero means it won't trigger at all.
        /// </param>
        /// <remarks>
        /// <para>
        /// Create a new evaluator using the specified time threshold in seconds.
        /// </para>
        /// <para>
        /// This evaluator will trigger if the specified time period
        /// <see cref="Interval"/> has passed since last check.
        /// </para>
        /// </remarks>
        public TimeEvaluator(int interval)
        {
            this.m_interval = interval;
            this.m_lastTimeUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets or sets the time threshold in seconds to trigger after.
        /// </summary>
        /// <value>
        /// The time threshold in seconds to trigger after.
        /// Zero means it won't trigger at all.
        /// </value>
        /// <remarks>
        /// <para>
        /// This evaluator will trigger if the specified time period
        /// <see cref="Interval"/> has passed since last check.
        /// </para>
        /// </remarks>
        public int Interval
        {
            get { return this.m_interval; }
            set { this.m_interval = value; }
        }

        /// <summary>
        /// Is this <paramref name="loggingEvent"/> the triggering event?.
        /// </summary>
        /// <param name="loggingEvent">The event to check.</param>
        /// <returns>This method returns <c>true</c>, if the specified time period
        /// <see cref="Interval"/> has passed since last check..
        /// Otherwise it returns <c>false</c>.</returns>
        /// <remarks>
        /// <para>
        /// This evaluator will trigger if the specified time period
        /// <see cref="Interval"/> has passed since last check.
        /// </para>
        /// </remarks>
        public bool IsTriggeringEvent(LoggingEvent loggingEvent)
        {
            if (loggingEvent == null)
            {
                throw new ArgumentNullException("loggingEvent");
            }

            // disable the evaluator if threshold is zero
            if (this.m_interval == 0)
            {
                return false;
            }

            lock (this) // avoid triggering multiple times
            {
                TimeSpan passed = DateTime.UtcNow.Subtract(this.m_lastTimeUtc);

                if (passed.TotalSeconds > this.m_interval)
                {
                    this.m_lastTimeUtc = DateTime.UtcNow;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
