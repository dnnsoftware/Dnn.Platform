// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Users
{
    public interface IUserEventHandlers
    {
        void UserAuthenticated(object sender, UserEventArgs args);

        void UserCreated(object sender, UserEventArgs args);

        void UserDeleted(object sender, UserEventArgs args);

        void UserRemoved(object sender, UserEventArgs args);

        void UserApproved(object sender, UserEventArgs args);

        void UserUpdated(object sender, UpdateUserEventArgs args);
    }
}
