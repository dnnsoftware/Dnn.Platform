// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Dto
{
    using System;
    using System.Collections.Generic;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Interfaces;
    using DotNetNuke.Entities.Users;
    using Newtonsoft.Json;

    /// <summary>
    /// Import/Export summary class to provide information about what will happen with this job.
    /// </summary>
    [JsonObject]
    public class ImportExportSummary : IDateTimeConverter
    {
        public ImportExportSummary()
        {
            this.SummaryItems = new SummaryList();
        }

        /// <summary>
        /// Gets formatted Date from which data was taken to perform export.
        /// </summary>
        public string FromDateString => Util.GetDateTimeString(this.FromDate);

        /// <summary>
        /// Gets formatted Date till which data was taken to perform export.
        /// </summary>
        public string ToDateString => Util.GetDateTimeString(this.ToDate);

        /// <summary>
        /// Gets or sets a value indicating whether does this import/export includes the properties definitions or not.
        /// </summary>
        public bool IncludeProfileProperties { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether does this import/export includes the permission or not.
        /// </summary>
        public bool IncludePermissions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether does this import/export includes the modules or not.
        /// </summary>
        public bool IncludeExtensions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether does this import/export includes the deleted items or not.
        /// </summary>
        public bool IncludeDeletions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether does this import/export includes content or not.
        /// </summary>
        public bool IncludeContent { get; set; }

        /// <summary>
        /// Gets or sets export mode. Differential or complete.
        /// </summary>
        public ExportMode ExportMode { get; set; }

        /// <summary>
        /// Gets or sets date from which data was taken to perform export.
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Gets or sets date till which data was taken to perform export.
        /// </summary>
        public DateTime ToDate { get; set; }

        /// <summary>
        /// Gets or sets summary of each item export.
        /// </summary>
        public IEnumerable<SummaryItem> SummaryItems { get; set; }

        /// <summary>
        /// Gets or sets exported file information.
        /// </summary>
        public ExportFileInfo ExportFileInfo { get; set; }

        public void ConvertToLocal(UserInfo userInfo)
        {
            if (userInfo == null)
            {
                return;
            }

            this.ToDate = Util.ToLocalDateTime(this.ToDate, userInfo);
            if (this.FromDate != null)
            {
                this.FromDate = Util.ToLocalDateTime(this.FromDate.Value, userInfo);
            }

            this.ExportFileInfo?.ConvertToLocal(userInfo);

            if (this.SummaryItems == null)
            {
                return;
            }

            var tempSummaryItems = new SummaryList();
            foreach (var summaryItem in this.SummaryItems)
            {
                summaryItem.ConvertToLocal(userInfo);
                tempSummaryItems.Add(summaryItem);
            }

            this.SummaryItems = tempSummaryItems;
        }
    }
}
