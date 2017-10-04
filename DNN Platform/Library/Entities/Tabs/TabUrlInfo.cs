using System;

namespace DotNetNuke.Entities.Tabs
{
    ///<summary>
    ///Class to represent a TabUrl object
    ///</summary>
    [Serializable] //584 support sql session state
    public class TabUrlInfo
    {
        #region Constructors

        public TabUrlInfo()
        {
            PortalAliasUsage = PortalAliasUsageType.Default;
        }

        #endregion

        #region Public Properties

        public string CultureCode { get; set; }
        public string HttpStatus { get; set; }
        public bool IsSystem { get; set; }
        public int PortalAliasId { get; set; }
        public PortalAliasUsageType PortalAliasUsage { get; set; }
        public string QueryString { get; set; }
        public int SeqNum { get; set; }
        public int TabId { get; set; }
        public int? CreatedByUserId { get; set; }
        public int? LastModifiedByUserId { get; set; }

        public string Url { get; set; }

        #endregion

    }
}