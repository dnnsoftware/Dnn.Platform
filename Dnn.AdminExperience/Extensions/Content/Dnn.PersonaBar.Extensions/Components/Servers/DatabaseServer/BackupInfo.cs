using System;

namespace Dnn.PersonaBar.Servers.Components.DatabaseServer
{
    public class BackupInfo
    {
        public string Name { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime FinishDate { get; set; }

        public long Size { get; set; }

        public string BackupType { get; set; }
    }
}
