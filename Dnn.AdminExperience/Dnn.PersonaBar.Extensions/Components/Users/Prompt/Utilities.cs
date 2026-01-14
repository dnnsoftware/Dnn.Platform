// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Users.Components.Prompt
{
    using Dnn.PersonaBar.Library.Prompt.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>Utility methods for the users prompt commands.</summary>
    public partial class Utilities
    {
        private static readonly UserValidator UserValidator = new UserValidator();

        /// <inheritdoc cref="IUserValidator.ValidateUser"/>
        [DnnDeprecated(9, 2, 1, "Use IUserValidator.ValidateUser.")]
        public static partial ConsoleErrorResultModel ValidateUser(int? userId, PortalSettings portalSettings, UserInfo currentUserInfo, out UserInfo userInfo)
        {
            return UserValidator.ValidateUser(userId, portalSettings, currentUserInfo, out userInfo);
        }
    }
}
