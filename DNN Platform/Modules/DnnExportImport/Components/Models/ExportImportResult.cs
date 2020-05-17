// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;
using System.Linq;
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Dto.Jobs;
using DotNetNuke.Common.Utilities;

namespace Dnn.ExportImport.Components.Models
{
    public class ExportImportResult
    {
        public int JobId { get; set; }
        public IList<LogItem> CompleteLog { get; private set; }

        public IList<LogItem> Summary
        {
            get { return CompleteLog.Where(item => item.ReportLevel >= ReportLevel.Info).ToList(); }
        }

        public ExportImportResult()
        {
            CompleteLog = CompleteLog = new List<LogItem>();
        }

        public LogItem AddSummary(string name, string value)
        {
            return AddLogEntry(name, value, ReportLevel.Info);
        }

        public LogItem AddLogEntry(string name, string value, ReportLevel level = ReportLevel.Verbose)
        {
            var item = new LogItem
            {
                Name = name,
                Value = value,
                ReportLevel = level,
                CreatedOnDate = DateUtils.GetDatabaseUtcTime(),
            };

            CompleteLog.Add(item);
            return item;
        }
    }
}
