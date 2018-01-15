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

using Dnn.PersonaBar.Library.DTO;
using Dnn.PersonaBar.Library.Helper;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Extensions.Components.Dto.Editors
{
    [JsonObject]
    public class PermissionsDto : Permissions
    {
        public PermissionsDto(bool needDefinitions) : base(needDefinitions)
        {
            foreach (var role in PermissionProvider.Instance().ImplicitRolesForPages(PortalSettings.Current.PortalId))
            {
                this.EnsureRole(role, true, true);
            }
        }

        [JsonProperty("desktopModuleId")]
        public int DesktopModuleId { get; set; }

        protected override void LoadPermissionDefinitions()
        {
            foreach (PermissionInfo permission in PermissionController.GetPermissionsByPortalDesktopModule())
            {
                PermissionDefinitions.Add(new Permission()
                {
                    PermissionId = permission.PermissionID,
                    PermissionName = permission.PermissionName,
                    FullControl = PermissionHelper.IsFullControl(permission),
                    View = PermissionHelper.IsViewPermisison(permission)
                });
            }
        }
    }
}