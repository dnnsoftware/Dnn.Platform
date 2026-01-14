// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Dto.Jobs
{
    using System;

    using Dnn.ExportImport.Components.Common;
    using Newtonsoft.Json;

    /// <summary>A data transfer object with information about a log item.</summary>
    [JsonObject]
    public class LogItem
    {
        /// <summary>Gets or sets the date the log entry was created.</summary>
        public DateTime CreatedOnDate { get; set; }

        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the value.</summary>
        public string Value { get; set; }

        /// <summary>Gets or sets the level.</summary>
        public ReportLevel ReportLevel { get; set; }
    }
}
