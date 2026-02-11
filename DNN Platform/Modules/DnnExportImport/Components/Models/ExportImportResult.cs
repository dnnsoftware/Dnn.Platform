// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Models
{
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Dto.Jobs;
    using DotNetNuke.Common.Utilities;

    /// <summary>An import/export result.</summary>
    public class ExportImportResult
    {
        /// <summary>Initializes a new instance of the <see cref="ExportImportResult"/> class.</summary>
        public ExportImportResult()
        {
            this.CompleteLog = this.CompleteLog = new List<LogItem>();
        }

        /// <summary>Gets the summary log items.</summary>
        public IList<LogItem> Summary
        {
            get { return this.CompleteLog.Where(item => item.ReportLevel >= ReportLevel.Info).ToList(); }
        }

        /// <summary>Gets or sets the job ID.</summary>
        public int JobId { get; set; }

        /// <summary>Gets the complete set of log items.</summary>
        public IList<LogItem> CompleteLog { get; private set; }

        /// <summary>Adds a summary log item.</summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns>The log item.</returns>
        public LogItem AddSummary(string name, string value)
        {
            return this.AddLogEntry(name, value, ReportLevel.Info);
        }

        /// <summary>Adds a log entry.</summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="level">The level.</param>
        /// <returns>The log item.</returns>
        public LogItem AddLogEntry(string name, string value, ReportLevel level = ReportLevel.Verbose)
        {
            var item = new LogItem
            {
                Name = name,
                Value = value,
                ReportLevel = level,
                CreatedOnDate = DateUtils.GetDatabaseUtcTime(),
            };

            this.CompleteLog.Add(item);
            return item;
        }
    }
}
