// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Attributes
{
    using System;
    using System.ComponentModel.DataAnnotations;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;

    [AttributeUsage(AttributeTargets.Property)]
    class UserExistAttribute : ValidationAttribute
    {
        public string[] RoleNames { get; set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var propertyName = validationContext.DisplayName;
            int userId;

            if (Int32.TryParse(value.ToString(), out userId))
            {
                var portalSetting = PortalController.Instance.GetCurrentPortalSettings();
                var user = UserController.Instance.GetUserById(portalSetting.PortalId, userId);

                if (user != null)
                {
                    foreach (var roleName in this.RoleNames)
                    {
                        if (!user.IsInRole(roleName))
                        {
                            return new ValidationResult(string.Format(Localization.GetString(Components.Constants.UserNotMemberOfRole, Components.Constants.LocalResourcesFile), roleName));
                        }
                    }

                    return ValidationResult.Success;
                }
            }

            return new ValidationResult(string.Format(Localization.GetString(Components.Constants.NotValid, Components.Constants.LocalResourcesFile), propertyName, value.ToString()));
        }
    }
}
