// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Attributes
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;

    [AttributeUsage(AttributeTargets.Property)]
    internal class UserExistAttribute : ValidationAttribute
    {
        public string[] RoleNames { get; set; }

        /// <inheritdoc/>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var propertyName = validationContext.DisplayName;

            if (int.TryParse(value.ToString(), out var userId))
            {
                var portalSetting = PortalController.Instance.GetCurrentSettings();
                var user = UserController.Instance.GetUserById(portalSetting.PortalId, userId);

                if (user != null)
                {
                    foreach (var roleName in this.RoleNames)
                    {
                        if (!user.IsInRole(roleName))
                        {
                            return new ValidationResult(string.Format(CultureInfo.CurrentCulture, Localization.GetString(Components.Constants.UserNotMemberOfRole, Components.Constants.LocalResourcesFile), roleName));
                        }
                    }

                    return ValidationResult.Success;
                }
            }

            return new ValidationResult(string.Format(CultureInfo.CurrentCulture, Localization.GetString(Components.Constants.NotValid, Components.Constants.LocalResourcesFile), propertyName, value.ToString()));
        }
    }
}
