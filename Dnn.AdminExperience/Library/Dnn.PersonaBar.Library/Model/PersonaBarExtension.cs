using System;
using System.Data;
using System.Runtime.Serialization;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;

namespace Dnn.PersonaBar.Library.Model
{
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

        public void Fill(IDataReader dr)
        {
            ExtensionId = Convert.ToInt32(dr["ExtensionId"]);
            Identifier = dr["Identifier"].ToString();
            FolderName = Null.SetNullString(dr["FolderName"]);
            MenuId = Convert.ToInt32(dr["MenuId"]);
            Controller = dr["Controller"].ToString();
            Container = dr["Container"].ToString();
            Path = dr["Path"].ToString();
            Order = Null.SetNullInteger(dr["Order"]);
            Enabled = Convert.ToBoolean(dr["Enabled"]);
        }

        public int KeyID { get { return ExtensionId; } set { ExtensionId = value; } }
    }
}
