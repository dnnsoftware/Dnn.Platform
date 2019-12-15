﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using System;
using System.ComponentModel.DataAnnotations;

namespace Dnn.PersonaBar.Security.Attributes
{
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
                    foreach (var roleName in RoleNames)
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
