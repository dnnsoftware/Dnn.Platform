// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.FileSystem;

namespace DotNetNuke.Modules.Journal
{
    public class UserFolderHelper
    {
        public UserFolderHelper(PortalSettings portalSettings)
        {
            UserFolder = FolderManager.Instance.GetUserFolder(portalSettings.UserInfo);
        }

        public IFolderInfo UserFolder { get; set; }

        public string UserFolderPhysicalPath
        {
            get
            {
                return UserFolder.PhysicalPath;
            }
        }

        public string UserFolderPath
        {
            get
            {
                return UserFolder.FolderPath;
            }
        }
    }
}
