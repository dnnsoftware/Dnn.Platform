// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.MvcPipeline.Models
{
    public class ModuleModelBase
    {
        public int ModuleId { get; set; }

        public int TabId { get; set; }

        public string LocalResourceFile { get; set; }
    }
}
