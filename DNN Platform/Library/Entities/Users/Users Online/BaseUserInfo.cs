// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users
{
    using System;

    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Users
    /// Class:      BaseUserInfo
    /// <summary>The BaseUserInfo class provides a base Entity for an online user.</summary>
    [Serializable]
    [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
    public abstract class BaseUserInfo
    {
        private DateTime creationDate;
        private DateTime lastActiveDate;
        private int portalID;
        private int tabID;

        /// <summary>Gets or sets the PortalId for this online user.</summary>
        [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
        public int PortalID
        {
            get
            {
                return this.portalID;
            }

            set
            {
                this.portalID = value;
            }
        }

        /// <summary>Gets or sets the TabId for this online user.</summary>
        [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
        public int TabID
        {
            get
            {
                return this.tabID;
            }

            set
            {
                this.tabID = value;
            }
        }

        /// <summary>Gets or sets the CreationDate for this online user.</summary>
        [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
        public DateTime CreationDate
        {
            get
            {
                return this.creationDate;
            }

            set
            {
                this.creationDate = value;
            }
        }

        /// <summary>Gets or sets the LastActiveDate for this online user.</summary>
        [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
        public DateTime LastActiveDate
        {
            get
            {
                return this.lastActiveDate;
            }

            set
            {
                this.lastActiveDate = value;
            }
        }
    }
}
