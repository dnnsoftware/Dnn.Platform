﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models
{
    using System.Collections.Generic;

    public class FolderViewModel
    {
        public FolderViewModel()
        {
            this.Attributes = new List<KeyValuePair<string, object>>();
        }

        public int FolderID { get; set; }

        public int FolderMappingID { get; set; }

        public string FolderName { get; set; }

        public string IconUrl { get; set; }

        public string FolderPath { get; set; }

        public string LastModifiedOnDate { get; set; }

        public int PortalID { get; set; }

        public bool HasChildren { get; set; }

        public IEnumerable<PermissionViewModel> Permissions { get; set; }

        public IList<KeyValuePair<string, object>> Attributes { get; set; }
    }
}
