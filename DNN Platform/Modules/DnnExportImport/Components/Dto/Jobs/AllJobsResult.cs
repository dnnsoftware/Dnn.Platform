#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017
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

using System;
using System.Collections.Generic;
using Dnn.ExportImport.Components.Interfaces;
using DotNetNuke.Entities.Users;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto.Jobs
{
    [JsonObject]
    public class AllJobsResult : IDateTimeConverter
    {
        public int PortalId { get; set; }
        public string PortalName { get; set; }
        public int TotalJobs { get; set; }
        public DateTime? LastExportTime { get; set; }
        public DateTime? LastImporTime { get; set; }
        public IEnumerable<JobItem> Jobs { get; set; }

        public void ConvertToLocal(UserInfo userInfo)
        {
            if (userInfo == null) return;
            if (Jobs == null) return;
            var tempJobs = new List<JobItem>();

            foreach (var job in Jobs)
            {
                job.ConvertToLocal(userInfo);
                tempJobs.Add(job);
            }
            Jobs = tempJobs;
        }
    }
}