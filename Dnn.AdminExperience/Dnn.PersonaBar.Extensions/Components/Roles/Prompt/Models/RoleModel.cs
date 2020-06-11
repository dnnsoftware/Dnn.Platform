// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
            this.ModifiedDate = role.LastModifiedOnDate.ToPromptLongDateString();
            this.CreatedDate = role.CreatedOnDate.ToPromptLongDateString();
            this.CreatedBy = role.CreatedByUserID;
            this.Description = role.Description;
        }
        #endregion

        #region Command Links
        public string __CreatedBy => $"get-user {this.CreatedBy}";

        #endregion
    }
}
