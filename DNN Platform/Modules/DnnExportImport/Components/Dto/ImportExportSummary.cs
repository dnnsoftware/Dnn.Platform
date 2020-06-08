// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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
            SummaryItems = new SummaryList();
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
        /// Does this import/export includes content or not.
        /// </summary>
        public bool IncludeContent { get; set; }

        /// <summary>
        /// Export mode. Differential or complete.
        /// </summary>
        public ExportMode ExportMode { get; set; }

        /// <summary>
        /// Date from which data was taken to perform export.
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Formatted Date from which data was taken to perform export.
        /// </summary>
        public string FromDateString => Util.GetDateTimeString(FromDate);

        /// <summary>
        /// Date till which data was taken to perform export.
        /// </summary>
        public DateTime ToDate { get; set; }

        /// <summary>
        /// Formatted Date till which data was taken to perform export.
        /// </summary>
        public string ToDateString => Util.GetDateTimeString(ToDate);

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
            var tempSummaryItems = new SummaryList();
            foreach (var summaryItem in SummaryItems)
            {
                summaryItem.ConvertToLocal(userInfo);
                tempSummaryItems.Add(summaryItem);
            }
            SummaryItems = tempSummaryItems;
        }
    }
}
