// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Security.Roles
{
    public interface IRoleEventHandlers
    {
        void RoleCreated(object sender, RoleEventArgs args);

        void RoleDeleted(object sender, RoleEventArgs args);

        void RoleJoined(object sender, RoleEventArgs args);

        void RoleLeft(object sender, RoleEventArgs args);
    }
}
