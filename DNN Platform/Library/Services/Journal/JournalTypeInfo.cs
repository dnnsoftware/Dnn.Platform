using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Common.Utilities;

namespace DotNetNuke.Services.Journal {
    [Serializable]
    public class JournalTypeInfo : IHydratable {
        public int JournalTypeId { get; set; }
        public int PortalId { get; set; }
        public string JournalType { get; set; }
        public string icon { get; set; }
        public bool AppliesToProfile { get; set; }
        public bool AppliesToGroup { get; set; }
        public bool AppliesToStream { get; set; }
        public bool SupportsNotify { get; set; }
        public string Options { get; set; }
        public bool IsEnabled { get; set; }
        public bool EnableComments { get; set; }
        public int KeyID {
            get {
                return JournalTypeId;
            }
            set {
                JournalTypeId = value;
            }
        }

        public void Fill(System.Data.IDataReader dr) {
            JournalTypeId = Null.SetNullInteger(dr["JournalTypeId"]);
            PortalId = Null.SetNullInteger(dr["PortalId"]);
            JournalType = Null.SetNullString(dr["JournalType"]);
            icon = Null.SetNullString(dr["icon"]);
            AppliesToProfile = Null.SetNullBoolean(dr["AppliesToProfile"]);
            AppliesToGroup = Null.SetNullBoolean(dr["AppliesToGroup"]);
            AppliesToStream = Null.SetNullBoolean(dr["AppliesToStream"]);
            SupportsNotify = Null.SetNullBoolean(dr["SupportsNotify"]);
            Options = Null.SetNullString(dr["Options"]);
            IsEnabled = Null.SetNullBoolean(dr["IsEnabled"]);
            EnableComments = Null.SetNullBoolean(dr["EnableComments"]);
        }
    }
}
