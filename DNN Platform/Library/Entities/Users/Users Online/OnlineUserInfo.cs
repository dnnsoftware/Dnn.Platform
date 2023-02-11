// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users
{
    using System;

    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Users
    /// Class:      OnlineUserInfo
    /// <summary>The OnlineUserInfo class provides an Entity for an online user.</summary>
    [Serializable]
    [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
    public class OnlineUserInfo : BaseUserInfo
    {
        private int userID;

        /// <summary>Gets or sets the User Id for this online user.</summary>
        [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
        public int UserID
        {
            get
            {
                return this.userID;
            }

            set
            {
                this.userID = value;
            }
        }
    }
}
