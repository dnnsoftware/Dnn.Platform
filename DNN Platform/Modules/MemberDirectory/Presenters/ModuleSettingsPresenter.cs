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
    [Obsolete("Deprecated in DNN 9.2.0. Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead")]
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