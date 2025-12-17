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
        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected override Func<IRegistrationProfileController> GetFactory()
        {
            return () => new RegistrationProfileController();
        }

        private static void AddProperty(List<string> results, string field, string searchTerm)
        {
            if (field.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant().Trim()))
            {
                results.Add(field);
            }
        }
    }
}
