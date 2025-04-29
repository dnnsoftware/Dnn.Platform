// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Roles.Components.Prompt.Models;

using System.Diagnostics.CodeAnalysis;

using Dnn.PersonaBar.Library.Prompt.Common;
using DotNetNuke.Security.Roles;

public class RoleModel : RoleModelBase
{
    /// <summary>Initializes a new instance of the <see cref="RoleModel"/> class.</summary>
    public RoleModel()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="RoleModel"/> class.</summary>
    /// <param name="role"></param>
    public RoleModel(RoleInfo role)
        : base(role)
    {
        this.ModifiedDate = role.LastModifiedOnDate.ToPromptLongDateString();
        this.CreatedDate = role.CreatedOnDate.ToPromptLongDateString();
        this.CreatedBy = role.CreatedByUserID;
        this.Description = role.Description;
    }

    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

    // ReSharper disable once InconsistentNaming
    public string __CreatedBy => $"get-user {this.CreatedBy}";

    public string Description { get; set; }

    public string CreatedDate { get; set; }

    public int CreatedBy { get; set; }
}
