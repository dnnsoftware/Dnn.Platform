// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Helper
{
    using System.Collections.Generic;

    using DotNetNuke.Services.Localization;

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
