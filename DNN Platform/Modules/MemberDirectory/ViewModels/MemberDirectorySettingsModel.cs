// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.MemberDirectory.ViewModels
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Web.Mvp;

    [Obsolete("Deprecated in DNN 9.2.0. Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead. Scheduled removal in v11.0.0.")]
    public class MemberDirectorySettingsModel : SettingsModel
    {
        public IList<ProfilePropertyDefinition> ProfileProperties;
        public IList<Relationship> Relationships;
        public IList<RoleInfo> Groups;
    }
}
