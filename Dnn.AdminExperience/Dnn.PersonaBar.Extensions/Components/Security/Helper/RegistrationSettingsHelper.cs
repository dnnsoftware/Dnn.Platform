// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Services.Localization;
using System.Collections.Generic;

namespace Dnn.PersonaBar.Security.Helper
{
    class RegistrationSettingsHelper
    {
        internal static List<KeyValuePair<string, int>> GetUserRegistrationOptions()
        {
            var userRegistrationOptions = new List<KeyValuePair<string, int>>();
            userRegistrationOptions.Add(new KeyValuePair<string, int>(Localization.GetString("None", Components.Constants.LocalResourcesFile), 0));
            userRegistrationOptions.Add(new KeyValuePair<string, int>(Localization.GetString("Private", Components.Constants.LocalResourcesFile), 1));
            userRegistrationOptions.Add(new KeyValuePair<string, int>(Localization.GetString("Public", Components.Constants.LocalResourcesFile), 2));
            userRegistrationOptions.Add(new KeyValuePair<string, int>(Localization.GetString("Verified", Components.Constants.LocalResourcesFile), 3));
            return userRegistrationOptions;
        }

        internal static List<KeyValuePair<string, int>> GetRegistrationFormOptions()
        {
            var registrationFormTypeOptions = new List<KeyValuePair<string, int>>();
            registrationFormTypeOptions.Add(new KeyValuePair<string, int>(Localization.GetString("Standard", Components.Constants.LocalResourcesFile), 0));
            registrationFormTypeOptions.Add(new KeyValuePair<string, int>(Localization.GetString("Custom", Components.Constants.LocalResourcesFile), 1));
            return registrationFormTypeOptions;
        }

    }
}
