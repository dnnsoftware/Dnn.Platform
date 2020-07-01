// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Journal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;

    [Serializable]
    public class JournalTypeInfo : IHydratable
    {
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

        public int KeyID
        {
            get
            {
                return this.JournalTypeId;
            }

            set
            {
                this.JournalTypeId = value;
            }
        }

        public void Fill(System.Data.IDataReader dr)
        {
            this.JournalTypeId = Null.SetNullInteger(dr["JournalTypeId"]);
            this.PortalId = Null.SetNullInteger(dr["PortalId"]);
            this.JournalType = Null.SetNullString(dr["JournalType"]);
            this.icon = Null.SetNullString(dr["icon"]);
            this.AppliesToProfile = Null.SetNullBoolean(dr["AppliesToProfile"]);
            this.AppliesToGroup = Null.SetNullBoolean(dr["AppliesToGroup"]);
            this.AppliesToStream = Null.SetNullBoolean(dr["AppliesToStream"]);
            this.SupportsNotify = Null.SetNullBoolean(dr["SupportsNotify"]);
            this.Options = Null.SetNullString(dr["Options"]);
            this.IsEnabled = Null.SetNullBoolean(dr["IsEnabled"]);
            this.EnableComments = Null.SetNullBoolean(dr["EnableComments"]);
        }
    }
}
