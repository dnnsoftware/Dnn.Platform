// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Servers.Components.DatabaseServer
{
    using System.Collections.Generic;

    using DotNetNuke.Common.Utilities;

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
