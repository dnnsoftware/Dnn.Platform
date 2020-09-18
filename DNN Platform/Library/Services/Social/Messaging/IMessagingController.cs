// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Messaging
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;

    public interface IMessagingController
    {
        void SendMessage(Message message, IList<RoleInfo> roles, IList<UserInfo> users, IList<int> fileIDs);

        void SendMessage(Message message, IList<RoleInfo> roles, IList<UserInfo> users, IList<int> fileIDs, UserInfo sender);
    }
}
