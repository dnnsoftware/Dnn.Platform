// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.MemberDirectory.ViewModels;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users.Social;
using DotNetNuke.Internal.SourceGenerators;
using DotNetNuke.Security.Roles;
using DotNetNuke.Web.Mvp;

[DnnDeprecated(9, 2, 0, "Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead")]
public partial class MemberDirectorySettingsModel : SettingsModel
{
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public IList<ProfilePropertyDefinition> ProfileProperties;
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public IList<Relationship> Relationships;
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public IList<RoleInfo> Groups;
}
