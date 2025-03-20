// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Security.Models
{
    public class PermissionModel
    {
        public bool Enabled { get; set; }

        public string State { get; set; }

        public bool IsFullControl { get; set; }

        public bool IsView { get; set; }

        public bool Locked { get; set; }

        public bool SupportsDenyMode { get; set; }

        public string PermissionKey { get; set; }
    }
}
