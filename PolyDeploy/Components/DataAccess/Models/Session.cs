using DotNetNuke.ComponentModel.DataAnnotations;
using System;

namespace Cantarus.Modules.PolyDeploy.Components.DataAccess.Models
{
    public enum SessionStatus
    {
        NotStarted = 0,
        InProgess = 1,
        Complete = 2
    }

    [TableName("Cantarus_PolyDeploy_Sessions")]
    [PrimaryKey("SessionID")]
    public class Session
    {
        public int SessionID { get; set; }
        public string Guid { get; set; }
        public SessionStatus Status { get; set; }
        public string Response { get; set; }
        public DateTime LastUsed { get; set; }

        public Session() { }

        public Session(string guid)
        {
            Guid = guid;
            Status = SessionStatus.NotStarted;
            LastUsed = DateTime.Now;
        }
    }
}
