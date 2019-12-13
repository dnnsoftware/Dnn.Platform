// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Interfaces;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto
{
    /// <summary>
    /// Details of the summary item to show in the export/import summary and progress.
    /// </summary>
    [JsonObject]
    public class SummaryItem : IDateTimeConverter
    {
        /// <summary>
        /// Category of the import/export. Also identifier for localization
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Total items to import/export.
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Formatted total items.
        /// </summary>
        public string TotalItemsString => Util.FormatNumber(TotalItems);

        /// <summary>
        /// Items processed.
        /// </summary>
        public int ProcessedItems { get; set; }

        /// <summary>
        /// Is job finished or not yet.
        /// </summary>
        public bool Completed { get; set; }

        /// <summary>
        /// Formatted processed items.
        /// </summary>
        public string ProcessedItemsString => Util.FormatNumber(ProcessedItems);

        /// <summary>
        /// Progress in percentage.
        /// </summary>
        public int ProgressPercentage { get; set; }

        /// <summary>
        /// Order to show on UI.
        /// </summary>
        public uint Order { get; set; }

        public void ConvertToLocal(UserInfo userInfo)
        {
            //Nothing to convert.
        }
    }
}
