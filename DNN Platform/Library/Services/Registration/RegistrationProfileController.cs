// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common.Lists;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Framework;

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
                this.AddProperty(results, definition.PropertyName, searchTerm);
            }

            this.AddProperty(results, "Email", searchTerm);
            this.AddProperty(results, "DisplayName", searchTerm);
            this.AddProperty(results, "Username", searchTerm);
            this.AddProperty(results, "Password", searchTerm);
            this.AddProperty(results, "PasswordConfirm", searchTerm);
            this.AddProperty(results, "PasswordQuestion", searchTerm);
            this.AddProperty(results, "PasswordAnswer", searchTerm);

            return results;
        }

        protected override Func<IRegistrationProfileController> GetFactory()
        {
            return () => new RegistrationProfileController();
        }

        private void AddProperty(List<string> results, string field, string searchTerm)
        {
            if (field.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant().Trim()))
            {
                results.Add(field);
            }
        }
    }
}
