// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Upgrade.Internals;
using DotNetNuke.Services.Upgrade.Internals.Steps;

#endregion

namespace DotNetNuke.Services.Upgrade.InternalController.Steps
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// InstallSuperUserStep - Step that installs SuperUser Account
    /// </summary>
    /// -----------------------------------------------------------------------------    
    public class InstallSuperUserStep : BaseInstallationStep
    {
        #region Implementation of IInstallationStep

        /// <summary>
        /// Main method to execute the step
        /// </summary>        
        public override void Execute()
        {
            Percentage = 0;
            Status = StepStatus.Running;

            Details = Localization.Localization.GetString("CreateSuperUser", LocalInstallResourceFile);
            var installConfig = InstallController.Instance.GetInstallConfig();

            //if any super user (even deleted) is found - exit
            var superUsers = UserController.GetUsers(true, true, Null.NullInteger);
            if (superUsers != null && superUsers.Count > 0)
            {
                Details = "...";
                Status = StepStatus.Done;
                return;
            }

            //Set admin user to be a superuser
            var adminSuperUser = UserController.GetUserByName(0, installConfig.SuperUser.UserName);
            if (adminSuperUser != null)
            {
                adminSuperUser.IsSuperUser = true;
                adminSuperUser.Membership.UpdatePassword = false;
				//refresh the profile to get definitions for super user.
	            adminSuperUser.Profile = null;
				adminSuperUser.Profile.PreferredLocale = installConfig.SuperUser.Locale;
				adminSuperUser.Profile.PreferredTimeZone = TimeZoneInfo.Local;
                UserController.UpdateUser(0, adminSuperUser);
            }
            else
            {
                //Construct UserInfo object
                var superUser = new UserInfo
                                {
                                    PortalID = -1,
                                    FirstName = installConfig.SuperUser.FirstName,
                                    LastName = installConfig.SuperUser.LastName,
                                    Username = installConfig.SuperUser.UserName,
                                    DisplayName = installConfig.SuperUser.FirstName + " " + installConfig.SuperUser.LastName,
                                    Membership = {Password = installConfig.SuperUser.Password},
                                    Email = installConfig.SuperUser.Email,
                                    IsSuperUser = true
                                };
                superUser.Membership.Approved = true;

                superUser.Profile.FirstName = installConfig.SuperUser.FirstName;
                superUser.Profile.LastName = installConfig.SuperUser.LastName;
                superUser.Profile.PreferredLocale = installConfig.SuperUser.Locale;
                superUser.Profile.PreferredTimeZone = TimeZoneInfo.Local;
                superUser.Membership.UpdatePassword = false;
                
                //Create SuperUser if not present
                if (UserController.GetUserByName(superUser.PortalID, superUser.Username) == null)
                    UserController.CreateUser(ref superUser);
            }

            Details = Localization.Localization.GetString("CreatingSuperUser", LocalInstallResourceFile) + installConfig.SuperUser.UserName;

            Status = StepStatus.Done;
        }

        #endregion
    }
}
