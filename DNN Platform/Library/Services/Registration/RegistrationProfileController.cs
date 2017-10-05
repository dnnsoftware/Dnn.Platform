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