// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Servers.Components.DatabaseServer
{
    using System;

    public class BackupInfo
    {
        public string Name { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime FinishDate { get; set; }

        public long Size { get; set; }

        public string BackupType { get; set; }
    }
}
