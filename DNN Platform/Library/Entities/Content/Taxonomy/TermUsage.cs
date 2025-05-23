﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Taxonomy
{
    public class TermUsage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TermUsage"/> class.
        /// parameterless constructor, so that it can be used in CBO.
        /// </summary>
        public TermUsage()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="TermUsage"/> class.</summary>
        /// <param name="total">The total usage count.</param>
        /// <param name="month">The usage count for the month.</param>
        /// <param name="week">The usage count for the week.</param>
        /// <param name="day">The usage count for the day.</param>
        internal TermUsage(int total, int month, int week, int day)
        {
            this.TotalTermUsage = total;

            this.MonthTermUsage = month;

            this.WeekTermUsage = week;

            this.DayTermUsage = day;
        }

        public int TotalTermUsage { get; set; }

        public int MonthTermUsage { get; set; }

        public int WeekTermUsage { get; set; }

        public int DayTermUsage { get; set; }
    }
}
