// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Dto
{
    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Interfaces;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;
    using Newtonsoft.Json;

    /// <summary>
    /// Details of the summary item to show in the export/import summary and progress.
    /// </summary>
    [JsonObject]
    public class SummaryItem : IDateTimeConverter
    {
        /// <summary>
        /// Gets formatted total items.
        /// </summary>
        public string TotalItemsString => Util.FormatNumber(this.TotalItems);

        /// <summary>
        /// Gets formatted processed items.
        /// </summary>
        public string ProcessedItemsString => Util.FormatNumber(this.ProcessedItems);

        /// <summary>
        /// Gets or sets category of the import/export. Also identifier for localization.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets total items to import/export.
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Gets or sets items processed.
        /// </summary>
        public int ProcessedItems { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is job finished or not yet.
        /// </summary>
        public bool Completed { get; set; }

        /// <summary>
        /// Gets or sets progress in percentage.
        /// </summary>
        public int ProgressPercentage { get; set; }

        /// <summary>
        /// Gets or sets order to show on UI.
        /// </summary>
        public uint Order { get; set; }

        public void ConvertToLocal(UserInfo userInfo)
        {
            // Nothing to convert.
        }
    }
}
