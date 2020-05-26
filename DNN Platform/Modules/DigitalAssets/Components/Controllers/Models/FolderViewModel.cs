// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;

namespace DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models
{
    public class FolderViewModel
    {
        public FolderViewModel()
        {
            Attributes = new List<KeyValuePair<string, object>>(); 
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
