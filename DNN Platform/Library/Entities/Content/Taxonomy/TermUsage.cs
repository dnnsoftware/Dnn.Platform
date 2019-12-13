﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Entities.Content.Taxonomy
{
    public class TermUsage
    {
        public int TotalTermUsage { get; set; }
        
        public int MonthTermUsage { get; set; }

        public int WeekTermUsage { get; set; }

        public int DayTermUsage { get; set; }

		/// <summary>
		/// parameterless constructor, so that it can be used in CBO.
		/// </summary>
		public TermUsage()
		{
			
		}

        internal TermUsage(int total, int month, int week, int day)
        {
            TotalTermUsage = total;

            MonthTermUsage = month;

            WeekTermUsage = week;

            DayTermUsage = day;
        }
    }
}
