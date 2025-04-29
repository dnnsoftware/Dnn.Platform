// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Roles.Components.Prompt.Models;

using System.Diagnostics.CodeAnalysis;

using Dnn.PersonaBar.Library.Prompt.Common;
using DotNetNuke.Security.Roles;

public class RoleModelBase
{
    public RoleModelBase()
    {
    }

    public RoleModelBase(RoleInfo role)
    {
        this.AutoAssign = role.AutoAssignment;
        this.ModifiedDate = role.LastModifiedOnDate.ToPromptShortDateString();
        this.ModifiedBy = role.LastModifiedByUserID;
        this.IsPublic = role.IsPublic;
        this.RoleGroupId = role.RoleGroupID;
        this.RoleId = role.RoleID;
        this.RoleName = role.RoleName;
        this.UserCount = role.UserCount;
    }

    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

    // ReSharper disable InconsistentNaming
    public string __ModifiedBy => $"get-user {this.ModifiedBy}";

    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
    public string __RoleId => $"get-role {this.RoleId}";

    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
    public string __UserCount => $"list-users --role '{this.RoleName}'";

    // ReSharper restore InconsistentNaming
    public int RoleId { get; set; }

    public int RoleGroupId { get; set; }

    public string RoleName { get; set; }

    public bool IsPublic { get; set; }

    public bool AutoAssign { get; set; }

    public int UserCount { get; set; }

    public string ModifiedDate { get; set; }

    public int ModifiedBy { get; set; }
}
