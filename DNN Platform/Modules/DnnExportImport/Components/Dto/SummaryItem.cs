#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2018
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
