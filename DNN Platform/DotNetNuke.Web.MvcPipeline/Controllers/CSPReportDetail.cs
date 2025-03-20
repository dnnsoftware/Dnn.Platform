// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Controllers
{
    using System;
    using System.IO;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Host;
    using DotNetNuke.Instrumentation;
    using Newtonsoft.Json;

    public class CSPReportDetail
    {
        [JsonProperty("document-uri")]
        public string DocumentUri { get; set; }

        [JsonProperty("violated-directive")]
        public string ViolatedDirective { get; set; }

        [JsonProperty("blocked-uri")]
        public string BlockedUri { get; set; }
    }
}
