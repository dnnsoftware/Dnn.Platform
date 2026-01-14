// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Servers.Components.DatabaseServer
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.Common.Utilities;

    public class DatabaseController
    {
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public DbInfo GetDbInfo()
        {
            return CBO.FillObject<DbInfo>(DataService.GetDbInfo());
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public List<BackupInfo> GetDbBackups()
        {
            return CBO.FillCollection<BackupInfo>(DataService.GetDbBackups());
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public List<DbFileInfo> GetDbFileInfo()
        {
            return CBO.FillCollection<DbFileInfo>(DataService.GetDbFileInfo());
        }
    }
}
