// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Roles
{
    public interface IRoleEventHandlers
    {
        void RoleCreated(object sender, RoleEventArgs args);

        void RoleDeleted(object sender, RoleEventArgs args);

        void RoleJoined(object sender, RoleEventArgs args);

        void RoleLeft(object sender, RoleEventArgs args);
    }

    /// <summary>
    /// The base interface <see cref="IRoleEventHandlers"/> has to be implemented by the class as well for the handler to be picked up.
    /// </summary>
    public interface IRoleUpdateEventHandlers
    {
        void RoleUpdated(object sender, RoleEventArgs args);
    }
}
