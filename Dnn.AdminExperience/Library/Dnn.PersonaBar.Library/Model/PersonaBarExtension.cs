// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Model
{
    using System;
    using System.Data;
    using System.Runtime.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;

    [DataContract]
    [Serializable]
    public class PersonaBarExtension : IHydratable
    {
        [DataMember]
        public int ExtensionId { get; set; }

        [DataMember]
        public string Identifier { get; set; }

        [DataMember]
        public int MenuId { get; set; }

        [DataMember]
        public string FolderName { get; set; }

        [IgnoreDataMember]
        public string Controller { get; set; }

        [DataMember]
        public string Container { get; set; }

        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public int Order { get; set; }

        [DataMember]
        public bool Enabled { get; set; }

        public int KeyID
        {
            get { return this.ExtensionId; }
            set { this.ExtensionId = value; }
        }

        public void Fill(IDataReader dr)
        {
            this.ExtensionId = Convert.ToInt32(dr["ExtensionId"]);
            this.Identifier = dr["Identifier"].ToString();
            this.FolderName = Null.SetNullString(dr["FolderName"]);
            this.MenuId = Convert.ToInt32(dr["MenuId"]);
            this.Controller = dr["Controller"].ToString();
            this.Container = dr["Container"].ToString();
            this.Path = dr["Path"].ToString();
            this.Order = Null.SetNullInteger(dr["Order"]);
            this.Enabled = Convert.ToBoolean(dr["Enabled"]);
        }
    }
}
