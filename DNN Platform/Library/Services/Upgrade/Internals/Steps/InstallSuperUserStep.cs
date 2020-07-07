// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Upgrade.InternalController.Steps
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Upgrade.Internals;
    using DotNetNuke.Services.Upgrade.Internals.Steps;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// InstallSuperUserStep - Step that installs SuperUser Account.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class InstallSuperUserStep : BaseInstallationStep
    {
        /// <summary>
        /// Main method to execute the step.
        /// </summary>
        public override void Execute()
        {
            this.Percentage = 0;
            this.Status = StepStatus.Running;

            this.Details = Localization.GetString("CreateSuperUser", this.LocalInstallResourceFile);
            var installConfig = InstallController.Instance.GetInstallConfig();

            // if any super user (even deleted) is found - exit
            var superUsers = UserController.GetUsers(true, true, Null.NullInteger);
            if (superUsers != null && superUsers.Count > 0)
            {
                this.Details = "...";
                this.Status = StepStatus.Done;
                return;
            }

            // Set admin user to be a superuser
            var adminSuperUser = UserController.GetUserByName(0, installConfig.SuperUser.UserName);
            if (adminSuperUser != null)
            {
                adminSuperUser.IsSuperUser = true;
                adminSuperUser.Membership.UpdatePassword = false;

                // refresh the profile to get definitions for super user.
                adminSuperUser.Profile = null;
                adminSuperUser.Profile.PreferredLocale = installConfig.SuperUser.Locale;
                adminSuperUser.Profile.PreferredTimeZone = TimeZoneInfo.Local;
                UserController.UpdateUser(0, adminSuperUser);
            }
            else
            {
                // Construct UserInfo object
                var superUser = new UserInfo
                {
                    PortalID = -1,
                    FirstName = installConfig.SuperUser.FirstName,
                    LastName = installConfig.SuperUser.LastName,
                    Username = installConfig.SuperUser.UserName,
                    DisplayName = installConfig.SuperUser.FirstName + " " + installConfig.SuperUser.LastName,
                    Membership = { Password = installConfig.SuperUser.Password },
                    Email = installConfig.SuperUser.Email,
                    IsSuperUser = true,
                };
                superUser.Membership.Approved = true;

                superUser.Profile.FirstName = installConfig.SuperUser.FirstName;
                superUser.Profile.LastName = installConfig.SuperUser.LastName;
                superUser.Profile.PreferredLocale = installConfig.SuperUser.Locale;
                superUser.Profile.PreferredTimeZone = TimeZoneInfo.Local;
                superUser.Membership.UpdatePassword = false;

                // Create SuperUser if not present
                if (UserController.GetUserByName(superUser.PortalID, superUser.Username) == null)
                {
                    UserController.CreateUser(ref superUser);
                }
            }

            this.Details = Localization.GetString("CreatingSuperUser", this.LocalInstallResourceFile) + installConfig.SuperUser.UserName;

            this.Status = StepStatus.Done;
        }
    }
}
