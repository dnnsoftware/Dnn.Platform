// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using Dnn.PersonaBar.Library.Prompt.Common;
using DotNetNuke.Security.Roles;

namespace Dnn.PersonaBar.Roles.Components.Prompt.Models
{
    public class RoleModel : RoleModelBase
    {
        public string Description { get; set; }
        public string CreatedDate { get; set; }
        public int CreatedBy { get; set; }

        #region Constructors
        public RoleModel()
        {
        }
        public RoleModel(RoleInfo role) : base(role)
        {
            ModifiedDate = role.LastModifiedOnDate.ToPromptLongDateString();
            CreatedDate = role.CreatedOnDate.ToPromptLongDateString();
            CreatedBy = role.CreatedByUserID;
            Description = role.Description;
        }
        #endregion

        #region Command Links
        public string __CreatedBy => $"get-user {CreatedBy}";

        #endregion
    }
}
