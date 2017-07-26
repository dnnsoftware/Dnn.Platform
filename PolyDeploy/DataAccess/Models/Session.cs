using DotNetNuke.ComponentModel.DataAnnotations;
using System;

namespace Cantarus.Modules.PolyDeploy.DataAccess.Models
{
    [TableName("Cantarus_PolyDeploy_Sessions")]
    [PrimaryKey("SessionID")]
    public class Session
    {
        public int SessionID { get; set; }
        public string Guid { get; set; }
        public DateTime LastUsed { get; set; }

        public Session() { }

        public Session(string guid)
        {
            Guid = guid;
            LastUsed = DateTime.Now;
        }
    }
}
