#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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