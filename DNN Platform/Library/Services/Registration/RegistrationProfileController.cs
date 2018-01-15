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
using System.Collections.Generic;
using DotNetNuke.Framework;
using DotNetNuke.Common.Lists;
using DotNetNuke.Entities.Profile;
using System.Linq;

namespace DotNetNuke.Services.Registration
{
    public class RegistrationProfileController : ServiceLocator<IRegistrationProfileController, RegistrationProfileController>, IRegistrationProfileController
    {
        public IEnumerable<string> Search(int portalId, string searchTerm)
        {
            var controller = new ListController();

            ListEntryInfo imageType = controller.GetListEntryInfo("DataType", "Image");

            List<string> results = new List<string>();
            foreach (var definition in ProfileController.GetPropertyDefinitionsByPortal(portalId)
                                        .Cast<ProfilePropertyDefinition>()
                                        .Where(definition => definition.DataType != imageType.EntryID))
            {
                AddProperty(results, definition.PropertyName, searchTerm);
            }

            AddProperty(results, "Email", searchTerm);
            AddProperty(results, "DisplayName", searchTerm);
            AddProperty(results, "Username", searchTerm);
            AddProperty(results, "Password", searchTerm);
            AddProperty(results, "PasswordConfirm", searchTerm);
            AddProperty(results, "PasswordQuestion", searchTerm);
            AddProperty(results, "PasswordAnswer", searchTerm);

            return results;            
        }

        private void AddProperty(List<string> results, string field, string searchTerm)
        {         
            if (field.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant().Trim()))
            {
                results.Add(field);
            }
        }

        protected override Func<IRegistrationProfileController> GetFactory()
        {
            return () => new RegistrationProfileController();
        }
    }
}