// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using Dnn.ExportImport.Components.Interfaces;
using DotNetNuke.Entities.Users;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto
{
    /// <summary>
    /// Export file(s) info.
    /// </summary>
    [JsonObject]
    public class ExportFileInfo : IDateTimeConverter
    {
        /// <summary>
        /// Path for exported files.
        /// </summary>
        public string ExportPath { get; set; }
        /// <summary>
        /// Formatted size of export file
        /// </summary>
        public string ExportSize { get; set; }

        public void ConvertToLocal(UserInfo userInfo)
        {
            //Nothing to convert.
        }
    }
}
