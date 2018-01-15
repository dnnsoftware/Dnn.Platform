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

using System;
using System.Linq;
using DotNetNuke.Services.Localization;
using System.ComponentModel.DataAnnotations;

namespace Dnn.PersonaBar.Security.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    class UserRegistrationOptionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var propertyName = validationContext.DisplayName;
            int userRegistrationOptionId;

            if (string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult(string.Format(Localization.GetString(Components.Constants.EmptyValue, Components.Constants.LocalResourcesFile), propertyName));
            }

            if (!Int32.TryParse(value.ToString(), out userRegistrationOptionId))
            {
                return new ValidationResult(string.Format(Localization.GetString(Components.Constants.NotValid, Components.Constants.LocalResourcesFile), propertyName, value.ToString()));
            }

            var allOptions = Helper.RegistrationSettingsHelper.GetUserRegistrationOptions();

            if (!allOptions.Select(o => o.Value).Contains(userRegistrationOptionId))
            {
                return new ValidationResult(string.Format(Localization.GetString(Components.Constants.NotValid, Components.Constants.LocalResourcesFile), propertyName, value.ToString()));
            }

            return ValidationResult.Success;
        }
    }
}