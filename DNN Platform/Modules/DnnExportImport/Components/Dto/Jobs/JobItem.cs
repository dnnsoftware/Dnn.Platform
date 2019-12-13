// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Interfaces;
using DotNetNuke.Entities.Users;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto.Jobs
{
    [JsonObject]
    public class JobItem : IDateTimeConverter
    {
        public int JobId { get; set; }
        public int PortalId { get; set; }
        public string User { get; set; }
        public string JobType { get; set; }
        public int Status { get; set; }
        public bool Cancelled { get; set; }
        public string JobStatus { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedOnString => Util.GetDateTimeString(CreatedOn);
        public DateTime? CompletedOn { get; set; }
        public string CompletedOnString => Util.GetDateTimeString(CompletedOn);

        public string ExportFile { get; set; }
        //public IEnumerable<LogItem> Summary { get; set; }
        public ImportExportSummary Summary { get; set; }

        public void ConvertToLocal(UserInfo userInfo)
        {
            if (userInfo == null) return;
            Summary?.ConvertToLocal(userInfo);
            CreatedOn = Util.ToLocalDateTime(CreatedOn, userInfo);
            if (CompletedOn != null)
                CompletedOn = Util.ToLocalDateTime(CompletedOn.Value, userInfo);
        }
    }
}
