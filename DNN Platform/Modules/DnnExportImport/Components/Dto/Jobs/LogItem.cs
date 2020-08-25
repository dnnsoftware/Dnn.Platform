// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Dto.Jobs
{
    using System;

    using Dnn.ExportImport.Components.Common;
    using Newtonsoft.Json;

    [JsonObject]
    public class LogItem
    {
        public DateTime CreatedOnDate { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public ReportLevel ReportLevel { get; set; }
    }
}
