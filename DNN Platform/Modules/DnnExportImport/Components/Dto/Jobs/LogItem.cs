// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using Dnn.ExportImport.Components.Common;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto.Jobs
{
    [JsonObject]
    public class LogItem
    {
        public DateTime CreatedOnDate { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public ReportLevel ReportLevel { get; set; }
    }
}
