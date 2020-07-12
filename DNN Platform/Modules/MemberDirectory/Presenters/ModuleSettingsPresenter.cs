// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.MemberDirectory.Presenters
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Common.Lists;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Modules.MemberDirectory.ViewModels;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Web.Mvp;

    [Obsolete("Deprecated in DNN 9.2.0. Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead. Scheduled removal in v11.0.0.")]
    public class ModuleSettingsPresenter : ModuleSettingsPresenter<ISettingsView<MemberDirectorySettingsModel>, MemberDirectorySettingsModel>
    {
        public ModuleSettingsPresenter(ISettingsView<MemberDirectorySettingsModel> view)
            : base(view)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            this.View.Model.Groups = RoleController.Instance.GetRoles(this.PortalId, r => r.Status == RoleStatus.Approved);
            this.View.Model.Relationships = RelationshipController.Instance.GetRelationshipsByPortalId(this.PortalId);

            this.View.Model.ProfileProperties = new List<ProfilePropertyDefinition>();
            foreach (ProfilePropertyDefinition definition in ProfileController.GetPropertyDefinitionsByPortal(this.PortalId))
            {
                var controller = new ListController();
                ListEntryInfo textType = controller.GetListEntryInfo("DataType", "Text");
                ListEntryInfo regionType = controller.GetListEntryInfo("DataType", "Region");
                ListEntryInfo countryType = controller.GetListEntryInfo("DataType", "Country");
                if (definition.DataType == textType.EntryID || definition.DataType == regionType.EntryID || definition.DataType == countryType.EntryID)
                {
                    this.View.Model.ProfileProperties.Add(definition);
                }
            }
        }
    }
}
