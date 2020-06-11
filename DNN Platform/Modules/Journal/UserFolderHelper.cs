// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.FileSystem;

namespace DotNetNuke.Modules.Journal
{
    public class UserFolderHelper
    {
        public UserFolderHelper(PortalSettings portalSettings)
        {
            this.UserFolder = FolderManager.Instance.GetUserFolder(portalSettings.UserInfo);
        }

        public IFolderInfo UserFolder { get; set; }

        public string UserFolderPhysicalPath
        {
            get
            {
                return this.UserFolder.PhysicalPath;
            }
        }

        public string UserFolderPath
        {
            get
            {
                return this.UserFolder.FolderPath;
            }
        }
    }
}
