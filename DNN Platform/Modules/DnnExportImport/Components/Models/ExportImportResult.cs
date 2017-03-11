#region Copyright
//
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions
// of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Dto.Jobs;

namespace Dnn.ExportImport.Components.Models
{
    public class ExportImportResult
    {
        public int JobId { get; set; }
        public JobStatus Status { get; set; }
        public IList<LogItem> Summary { get; private set; }
        public IList<LogItem> CompleteLog { get; private set; }

        public LogItem AddSummary(string name, string value)
        {
            // no worries about conncurrency; all jobs are executed serially
            if (Summary == null) Summary = new List<LogItem>();

            var item = AddLogEntry(name, value, true);
            Summary.Add(item);
            return item;
        }

        public LogItem AddLogEntry(string name, string value, bool isSummary = false)
        {
            // no worries about conncurrency; all jobs are executed serially
            if (CompleteLog == null) CompleteLog = new List<LogItem>();

            var item = new LogItem
            {
                Name = name,
                Value = value,
                IsSummary = isSummary,
                CreatedOnDate = DateTime.UtcNow,
            };

            CompleteLog.Add(item);
            return item;
        }
    }
}
