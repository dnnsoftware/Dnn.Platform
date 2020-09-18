// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users
{
    using System;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Users
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The AnonymousUserInfo class provides an Entity for an anonymous user.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Serializable]
    [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
    public class AnonymousUserInfo : BaseUserInfo
    {
        private string _UserID;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the User Id for this online user.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
        public string UserID
        {
            get
            {
                return this._UserID;
            }

            set
            {
                this._UserID = value;
            }
        }
    }
}
