#region Usings

using System;
using System.Data;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Entities.Tabs
{
    ///<summary>
    ///Class to represent a TabAliasSkinInfo object
    ///</summary>
    [Serializable]
    public class TabAliasSkinInfo : BaseEntityInfo, IHydratable
    {
        #region Public Properties

        public int TabAliasSkinId { get; set; }
        public string HttpAlias { get; set; }
        public int PortalAliasId { get; set; }
        public string SkinSrc { get; set; }
        public int TabId { get; set; }

        #endregion

        public int KeyID 
        { 
            get { return TabAliasSkinId; } 
            set { TabAliasSkinId = value; }
        }

        public void Fill(IDataReader dr)
        {
            base.FillInternal(dr);

            TabAliasSkinId = Null.SetNullInteger(dr["TabAliasSkinId"]);
            HttpAlias = Null.SetNullString(dr["HttpAlias"]);
            PortalAliasId = Null.SetNullInteger(dr["PortalAliasId"]);
            SkinSrc = Null.SetNullString(dr["SkinSrc"]);
            TabId = Null.SetNullInteger(dr["TabId"]);
        }
    }
}
