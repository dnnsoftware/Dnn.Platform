// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;
using DotNetNuke.Common.Utilities;

namespace Dnn.PersonaBar.Servers.Components.DatabaseServer
{
    public class DatabaseController
    {
        public DbInfo GetDbInfo()
        {
            return CBO.FillObject<DbInfo>(DataService.GetDbInfo());
        }

        public List<BackupInfo> GetDbBackups()
        {
            return CBO.FillCollection<BackupInfo>(DataService.GetDbBackups());
        }

        public List<DbFileInfo> GetDbFileInfo()
        {
            return CBO.FillCollection<DbFileInfo>(DataService.GetDbFileInfo());
        }
    }
}
