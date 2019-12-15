// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;

namespace DotNetNuke.Services.Social.Messaging
{
    public interface IMessagingController
    {
        #region Public APIs

        void SendMessage(Message message, IList<RoleInfo> roles, IList<UserInfo> users, IList<int> fileIDs);

        void SendMessage(Message message, IList<RoleInfo> roles, IList<UserInfo> users, IList<int> fileIDs, UserInfo sender);

        #endregion        
    }
}
