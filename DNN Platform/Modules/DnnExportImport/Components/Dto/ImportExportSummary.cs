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
using Dnn.ExportImport.Components.Interfaces;
using DotNetNuke.Entities.Users;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto
{
    /// <summary>
    /// Import/Export summary class to provide information about what will happen with this job.
    /// </summary>
    [JsonObject]
    public class ImportExportSummary : IDateTimeConverter
    {
        public ImportExportSummary()
        {
            SummaryItems = new List<SummaryItem>();
        }
        /// <summary>
        /// Does this import/export includes the properties definitions or not.
        /// </summary>
        public bool IncludeProfileProperties { get; set; }
        /// <summary>
        /// Does this import/export includes the permission or not.
        /// </summary>
        public bool IncludePermissions { get; set; }
        /// <summary>
        /// Does this import/export includes the modules or not.
        /// </summary>
        public bool IncludeExtensions { get; set; }
        /// <summary>
        /// Does this import/export includes the deleted items or not.
        /// </summary>
        public bool IncludeDeletions { get; set; }

        /// <summary>
        /// Export mode. Differential or complete.
        /// </summary>
        public ExportMode ExportMode { get; set; }

        /// <summary>
        /// Date from which data was taken to perform export.
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Date till which data was taken to perform export.
        /// </summary>
        public DateTime ToDate { get; set; }

        /// <summary>
        /// Summary of each item export.
        /// </summary>
        public IEnumerable<SummaryItem> SummaryItems { get; set; }
        /// <summary>
        /// Exported file information.
        /// </summary>
        public ExportFileInfo ExportFileInfo { get; set; }

        public void ConvertToLocal(UserInfo userInfo)
        {
            if (userInfo == null) return;
            ToDate = Util.ToLocalDateTime(ToDate, userInfo);
            if (FromDate != null)
                FromDate = Util.ToLocalDateTime(FromDate.Value, userInfo);
            ExportFileInfo?.ConvertToLocal(userInfo);

            if (SummaryItems == null) return;
            var tempSummaryItems = new List<SummaryItem>();
            foreach (var summaryItem in SummaryItems)
            {
                summaryItem.ConvertToLocal(userInfo);
                tempSummaryItems.Add(summaryItem);
            }
            SummaryItems = tempSummaryItems;
        }
    }
}
