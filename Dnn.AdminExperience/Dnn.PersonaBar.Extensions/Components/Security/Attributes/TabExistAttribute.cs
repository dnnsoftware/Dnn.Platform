// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Attributes
{
    using System;
    using System.ComponentModel.DataAnnotations;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Localization;

    [AttributeUsage(AttributeTargets.Property)]
    class TabExistAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var propertyName = validationContext.DisplayName;
            int tabId = Convert.ToInt32(value.ToString());

            if (tabId != -1)
            {
                var portalSetting = PortalController.Instance.GetCurrentPortalSettings();
                var tab = TabController.Instance.GetTab(tabId, portalSetting.PortalId);

                if (tab == null)
                {
                    return new ValidationResult(string.Format(Localization.GetString(Components.Constants.NotValid, Components.Constants.LocalResourcesFile), propertyName, tabId));
                }

                if (tab.DisableLink)
                {
                    return new ValidationResult(string.Format(Localization.GetString(Components.Constants.DisabledTab, Components.Constants.LocalResourcesFile), tabId));
                }

                if (tab.IsDeleted)
                {
                    return new ValidationResult(string.Format(Localization.GetString(Components.Constants.DeletedTab, Components.Constants.LocalResourcesFile), tabId));
                }
            }

            return ValidationResult.Success;
        }
    }
}
