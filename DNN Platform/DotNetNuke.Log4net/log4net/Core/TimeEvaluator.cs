#region Copyright & License
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

namespace log4net.Core
{
    /// <summary>
    /// An evaluator that triggers after specified number of seconds.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This evaluator will trigger if the specified time period 
    /// <see cref="Interval"/> has passed since last check.
    /// </para>
    /// </remarks>
    /// <author>Robert Sevcik</author>
    public class TimeEvaluator : ITriggeringEventEvaluator
    {
        /// <summary>
        /// The time threshold for triggering in seconds. Zero means it won't trigger at all.
        /// </summary>
        private int m_interval;

        /// <summary>
        /// The time of last check. This gets updated when the object is created and when the evaluator triggers.
        /// </summary>
        private DateTime m_lasttime;

        /// <summary>
        /// The default time threshold for triggering in seconds. Zero means it won't trigger at all.
        /// </summary>
        const int DEFAULT_INTERVAL = 0;

        /// <summary>
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
            m_interval = interval;
            m_lasttime = DateTime.Now;
        }

        /// <summary>
        /// The time threshold in seconds to trigger after
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
            get { return m_interval; }
            set { m_interval = value; }
        }

        /// <summary>
        /// Is this <paramref name="loggingEvent"/> the triggering event?
        /// </summary>
        /// <param name="loggingEvent">The event to check</param>
        /// <returns>This method returns <c>true</c>, if the specified time period 
        /// <see cref="Interval"/> has passed since last check.. 
        /// Otherwise it returns <c>false</c></returns>
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
            if (m_interval == 0) return false;

            lock (this) // avoid triggering multiple times
            {
                TimeSpan passed = DateTime.Now.Subtract(m_lasttime);

                if (passed.TotalSeconds > m_interval)
                {
                    m_lasttime = DateTime.Now;
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
