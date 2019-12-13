// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;

namespace DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models
{
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
