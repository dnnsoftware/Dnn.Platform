// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.ResourceManager.Services.Dto
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    using Dnn.Modules.ResourceManager.Components;

    /// <summary>
    /// Represents a permissions set.
    /// </summary>
    [DataContract]
    public abstract class Permissions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Permissions"/> class.
        /// </summary>
        protected Permissions()
            : this(false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Permissions"/> class.
        /// </summary>
        /// <param name="needDefinitions">A value indicating whether the permissions need to be loaded.</param>
        protected Permissions(bool needDefinitions)
        {
            this.RolePermissions = new List<RolePermission>();
            this.UserPermissions = new List<UserPermission>();

            if (needDefinitions)
            {
                this.PermissionDefinitions = new List<Permission>();
                this.LoadPermissionDefinitions();
                this.EnsureDefaultRoles();
            }
        }

        /// <summary>
        /// Gets or sets the list of permissions definitions.
        /// </summary>
        [DataMember(Name = "permissionDefinitions")]
        public IList<Permission> PermissionDefinitions { get; set; }

        /// <summary>
        /// Gets or sets a list of role based permissions.
        /// </summary>
        [DataMember(Name = "rolePermissions")]
        public IList<RolePermission> RolePermissions { get; set; }

        /// <summary>
        /// Gets or sets a list of user based permissions.
        /// </summary>
        [DataMember(Name = "userPermissions")]
        public IList<UserPermission> UserPermissions { get; set; }

        /// <summary>
        /// Loads the permissions definitions.
        /// </summary>
        protected abstract void LoadPermissionDefinitions();
    }
}
