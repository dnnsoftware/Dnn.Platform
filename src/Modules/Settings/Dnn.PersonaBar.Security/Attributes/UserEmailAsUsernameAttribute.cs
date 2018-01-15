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
using System.ComponentModel.DataAnnotations;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Security.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    class UserEmailAsUsernameAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var propertyName = validationContext.DisplayName;
            bool userEmailAsUsername;

            if (!bool.TryParse(value.ToString(), out userEmailAsUsername))
            {
                return new ValidationResult(string.Format(Localization.GetString(Components.Constants.NotValid, Components.Constants.LocalResourcesFile), propertyName, value.ToString()));
            }

            if (userEmailAsUsername && UserController.GetDuplicateEmailCount() > 0)
            {
                return new ValidationResult(Localization.GetString(Components.Constants.ContainsDuplicateAddresses, Components.Constants.LocalResourcesFile));
            }

            return ValidationResult.Success;
        }
    }
}