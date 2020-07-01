// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users
{
    using System;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Users
    /// Class:      BaseUserInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The BaseUserInfo class provides a base Entity for an online user.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Serializable]
    [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
    public abstract class BaseUserInfo
    {
        private DateTime _CreationDate;
        private DateTime _LastActiveDate;
        private int _PortalID;
        private int _TabID;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the PortalId for this online user.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
        public int PortalID
        {
            get
            {
                return this._PortalID;
            }

            set
            {
                this._PortalID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the TabId for this online user.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
        public int TabID
        {
            get
            {
                return this._TabID;
            }

            set
            {
                this._TabID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the CreationDate for this online user.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
        public DateTime CreationDate
        {
            get
            {
                return this._CreationDate;
            }

            set
            {
                this._CreationDate = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the LastActiveDate for this online user.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
        public DateTime LastActiveDate
        {
            get
            {
                return this._LastActiveDate;
            }

            set
            {
                this._LastActiveDate = value;
            }
        }
    }
}
