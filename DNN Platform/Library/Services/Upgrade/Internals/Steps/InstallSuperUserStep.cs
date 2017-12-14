#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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
