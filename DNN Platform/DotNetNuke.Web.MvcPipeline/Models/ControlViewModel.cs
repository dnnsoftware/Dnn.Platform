// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.MvcPipeline.Models
{
    public class ControlViewModel
    {
        public int ModuleId { get; set; }

        public int TabId { get; set; }

        public int ModuleControlId { get; set; }

        public string PanaName { get; set; }

        public string ContainerSrc { get; set; }

        public string ContainerPath { get; set; }

        public string IconFile { get; set; }
    }
}
