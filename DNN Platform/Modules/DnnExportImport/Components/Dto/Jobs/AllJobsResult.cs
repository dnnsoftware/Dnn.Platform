// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.ExportImport.Components.Dto.Jobs;

using System;
using System.Collections.Generic;

using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Interfaces;
using DotNetNuke.Entities.Users;
using Newtonsoft.Json;

/// <summary>A paged collection of <see cref="JobItem"/> instances.</summary>
[JsonObject]
public class AllJobsResult : IDateTimeConverter
{
    /// <summary>Gets the total job count as a string.</summary>
    public string TotalJobsString => Util.FormatNumber(this.TotalJobs);

    /// <summary>Gets the last export time as a string.</summary>
    public string LastExportTimeString => Util.GetDateTimeString(this.LastExportTime);

    /// <summary>Gets the last import time as a string.</summary>
    public string LastImportTimeString => Util.GetDateTimeString(this.LastImportTime);

    /// <summary>Gets or sets the portal ID.</summary>
    public int PortalId { get; set; }

    /// <summary>Gets or sets the portal name.</summary>
    public string PortalName { get; set; }

    /// <summary>Gets or sets the total number of jobs.</summary>
    public int TotalJobs { get; set; }

    /// <summary>Gets or sets the last export time.</summary>
    public DateTime? LastExportTime { get; set; }

    /// <summary>Gets or sets the last import time.</summary>
    public DateTime? LastImportTime { get; set; }

    /// <summary>Gets or sets the jobs (or <c>null</c> if there are no jobs).</summary>
    public IEnumerable<JobItem> Jobs { get; set; }

    /// <inheritdoc/>
    public void ConvertToLocal(UserInfo userInfo)
    {
        this.LastExportTime = Util.ToLocalDateTime(this.LastExportTime, userInfo);
        this.LastImportTime = Util.ToLocalDateTime(this.LastImportTime, userInfo);

        if (userInfo == null)
        {
            return;
        }

        if (this.Jobs == null)
        {
            return;
        }

        var tempJobs = new List<JobItem>();

        foreach (var job in this.Jobs)
        {
            job.ConvertToLocal(userInfo);
            tempJobs.Add(job);
        }

        this.Jobs = tempJobs;
    }
}
