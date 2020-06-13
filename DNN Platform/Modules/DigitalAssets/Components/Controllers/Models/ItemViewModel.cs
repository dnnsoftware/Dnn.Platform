// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models
{
    using System;
    using System.Collections.Generic;

    public class ItemViewModel : ItemBaseViewModel
    {
        public string ItemName { get; set; }

        public string IconUrl { get; set; }

        public string DisplayName { get; set; }

        public string LastModifiedOnDate { get; set; }

        public int PortalID { get; set; }

        public IEnumerable<PermissionViewModel> Permissions { get; set; }

        public string ParentFolder { get; set; }

        public int ParentFolderID { get; set; }

        public string Size { get; set; }

        public int? FolderMappingID { get; set; }

        public string UnlinkAllowedStatus { get; set; }
    }
}
