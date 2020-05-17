﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;

using DotNetNuke.Common.Lists;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users.Social;
using DotNetNuke.Modules.MemberDirectory.ViewModels;
using DotNetNuke.Security.Roles;
using DotNetNuke.Web.Mvp;

#endregion

namespace DotNetNuke.Modules.MemberDirectory.Presenters
{
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

            View.Model.Groups = RoleController.Instance.GetRoles(PortalId, r => r.Status == RoleStatus.Approved);
            View.Model.Relationships = RelationshipController.Instance.GetRelationshipsByPortalId(PortalId);

            View.Model.ProfileProperties = new List<ProfilePropertyDefinition>();
            foreach (ProfilePropertyDefinition definition in ProfileController.GetPropertyDefinitionsByPortal(PortalId))
            {
                var controller = new ListController();
                ListEntryInfo textType = controller.GetListEntryInfo("DataType", "Text");
                ListEntryInfo regionType = controller.GetListEntryInfo("DataType", "Region");
                ListEntryInfo countryType = controller.GetListEntryInfo("DataType", "Country");
                if (definition.DataType == textType.EntryID || definition.DataType == regionType.EntryID || definition.DataType == countryType.EntryID)
                {
                    View.Model.ProfileProperties.Add(definition);
                }
            }
        }
    }
}
