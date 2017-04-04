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

using System.Collections.Generic;
using System.Linq;
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
            get { return CompleteLog.Where(item => item.IsSummary).ToList(); }
        }

        public ExportImportResult()
        {
            CompleteLog = CompleteLog = new List<LogItem>();
        }

        public LogItem AddSummary(string name, string value)
        {
            return AddLogEntry(name, value, true);
        }

        public LogItem AddLogEntry(string name, string value)
        {
            return AddLogEntry(name, value, false);
        }

        private LogItem AddLogEntry(string name, string value, bool isSummary)
        {
            var item = new LogItem
            {
                Name = name,
                Value = value,
                IsSummary = isSummary,
                CreatedOnDate = DateUtils.GetDatabaseUtcTime(),
            };

            CompleteLog.Add(item);
            return item;
        }
    }
}
