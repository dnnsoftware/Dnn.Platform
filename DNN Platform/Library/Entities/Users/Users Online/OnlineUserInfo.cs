// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users
{
    using System;

    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>The OnlineUserInfo class provides an Entity for an online user.</summary>
    [Serializable]
    [DnnDeprecated(8, 0, 0, "Other solutions exist outside of the DNN Platform", RemovalVersion = 11)]
    public partial class OnlineUserInfo : BaseUserInfo
    {
        private int userID;

        /// <summary>Gets or sets the User Id for this online user.</summary>
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
