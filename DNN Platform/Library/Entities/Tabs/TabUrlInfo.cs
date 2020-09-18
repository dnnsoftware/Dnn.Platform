// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs
{
    using System;

    /// <summary>
    /// Class to represent a TabUrl object.
    /// </summary>
    [Serializable] // 584 support sql session state
    public class TabUrlInfo
    {
        public TabUrlInfo()
        {
            this.PortalAliasUsage = PortalAliasUsageType.Default;
        }

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
    }
}
