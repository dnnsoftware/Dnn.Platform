// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A permissions grid for desktop module permissions.</summary>
    public class DesktopModulePermissionsGrid : PermissionsGrid
    {
        private static readonly string[] PermissionKeySeparator = ["##",];
        private readonly IPermissionDefinitionService permissionDefinitionService;
        private DesktopModulePermissionCollection desktopModulePermissions;
        private List<IPermissionInfo> permissionCollection;
        private int portalDesktopModuleId = -1;

        /// <summary>Initializes a new instance of the <see cref="DesktopModulePermissionsGrid"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPermissionDefinitionService. Scheduled removal in v12.0.0.")]
        public DesktopModulePermissionsGrid()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DesktopModulePermissionsGrid"/> class.</summary>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        public DesktopModulePermissionsGrid(IPermissionDefinitionService permissionDefinitionService)
        {
            this.permissionDefinitionService = permissionDefinitionService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>();
        }

        /// <summary>Gets the Permissions Collection.</summary>
        public DesktopModulePermissionCollection Permissions
        {
            get
            {
                // First Update Permissions in case they have been changed
                this.UpdatePermissions();

                // Return the DesktopModulePermissions
                return this.desktopModulePermissions;
            }
        }

        /// <summary>Gets or sets the ID of the PortalDesktopModule.</summary>
        public int PortalDesktopModuleID
        {
            get
            {
                return this.portalDesktopModuleId;
            }

            set
            {
                int oldValue = this.portalDesktopModuleId;
                this.portalDesktopModuleId = value;
                if (this.desktopModulePermissions == null || oldValue != value)
                {
                    this.GetDesktopModulePermissions();
                }
            }
        }

        /// <inheritdoc />
        protected override bool SupportsPermissionsAbstractions => true;

        /// <inheritdoc />
        protected override IList<IPermissionInfo> PermissionCollection => this.permissionCollection ??= this.desktopModulePermissions?.Cast<IPermissionInfo>().ToList();

        /// <summary>Resets the permissions collection.</summary>
        public void ResetPermissions()
        {
            this.GetDesktopModulePermissions();
            this.permissionCollection = null;
        }

        /// <inheritdoc />
        public override void GenerateDataGrid()
        {
        }

        /// <inheritdoc />
        protected override void AddPermission(IPermissionDefinitionInfo permissionDefinition, int roleId, string roleName, int userId, string displayName, bool allowAccess)
        {
            var objPermission = new DesktopModulePermissionInfo(permissionDefinition)
            {
                PortalDesktopModuleID = this.PortalDesktopModuleID,
                RoleName = roleName,
                AllowAccess = allowAccess,
                DisplayName = displayName,
            };
            ((IPermissionInfo)objPermission).RoleId = roleId;
            ((IPermissionInfo)objPermission).UserId = userId;
            this.desktopModulePermissions.Add(objPermission, true);

            // Clear Permission List
            this.permissionCollection = null;
        }

        /// <inheritdoc />
        protected override void AddPermission(IList<IPermissionDefinitionInfo> permissionsList, UserInfo user)
        {
            // Search DesktopModulePermission Collection for the user
            bool isMatch = false;
            foreach (IPermissionInfo objDesktopModulePermission in this.desktopModulePermissions)
            {
                if (objDesktopModulePermission.UserId == user.UserID)
                {
                    isMatch = true;
                    break;
                }
            }

            // user not found so add new
            if (!isMatch)
            {
                foreach (var objPermission in permissionsList)
                {
                    if (objPermission.PermissionKey == "DEPLOY")
                    {
                        this.AddPermission(objPermission, int.Parse(Globals.glbRoleNothing, CultureInfo.InvariantCulture), Null.NullString, user.UserID, user.DisplayName, true);
                    }
                }
            }
        }

        /// <inheritdoc />
        protected override void AddPermission(IList<IPermissionDefinitionInfo> permissionsList, RoleInfo role)
        {
            // Search TabPermission Collection for the user
            if (this.desktopModulePermissions.Any((IPermissionInfo p) => p.RoleId == role.RoleID))
            {
                return;
            }

            // role not found so add new
            foreach (var objPermission in permissionsList)
            {
                if (objPermission.PermissionKey == "DEPLOY")
                {
                    this.AddPermission(objPermission, role.RoleID, role.RoleName, Null.NullInteger, Null.NullString, true);
                }
            }
        }

        /// <inheritdoc />
        protected override IList<IPermissionDefinitionInfo> GetPermissionDefinitions()
        {
            return this.permissionDefinitionService.GetDefinitionsByPortalDesktopModule().ToList();
        }

        /// <inheritdoc />
        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                // Load State from the array of objects that was saved with SaveViewState.
                var myState = (object[])savedState;

                // Load Base Controls ViewState
                if (myState[0] != null)
                {
                    base.LoadViewState(myState[0]);
                }

                // Load DesktopModuleId
                if (myState[1] != null)
                {
                    this.PortalDesktopModuleID = Convert.ToInt32(myState[1], CultureInfo.InvariantCulture);
                }

                // Load DesktopModulePermissions
                if (myState[2] != null)
                {
                    this.desktopModulePermissions = new DesktopModulePermissionCollection();
                    string state = Convert.ToString(myState[2], CultureInfo.InvariantCulture);
                    if (!string.IsNullOrEmpty(state))
                    {
                        // First Break the String into individual Keys
                        string[] permissionKeys = state.Split(PermissionKeySeparator, StringSplitOptions.None);
                        foreach (string key in permissionKeys)
                        {
                            string[] settings = key.Split('|');
                            this.desktopModulePermissions.Add(this.ParseKeys(settings));
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        protected override void RemovePermission(int permissionID, int roleID, int userID)
        {
            this.desktopModulePermissions.Remove(permissionID, roleID, userID);

            // Clear Permission List
            this.permissionCollection = null;
        }

        /// <inheritdoc />
        protected override object SaveViewState()
        {
            var allStates = new object[3];

            // Save the Base Controls ViewState
            allStates[0] = base.SaveViewState();

            // Save the DesktopModule ID
            allStates[1] = this.PortalDesktopModuleID;

            // Persist the DesktopModulePermissions
            var sb = new StringBuilder();
            if (this.desktopModulePermissions != null)
            {
                bool addDelimiter = false;
                foreach (DesktopModulePermissionInfo objDesktopModulePermission in this.desktopModulePermissions)
                {
                    if (addDelimiter)
                    {
                        sb.Append("##");
                    }
                    else
                    {
                        addDelimiter = true;
                    }

                    sb.Append(this.BuildKey(
                        objDesktopModulePermission.AllowAccess,
                        ((IPermissionInfo)objDesktopModulePermission).PermissionId,
                        objDesktopModulePermission.DesktopModulePermissionID,
                        ((IPermissionInfo)objDesktopModulePermission).RoleId,
                        objDesktopModulePermission.RoleName,
                        ((IPermissionInfo)objDesktopModulePermission).UserId,
                        objDesktopModulePermission.DisplayName));
                }
            }

            allStates[2] = sb.ToString();
            return allStates;
        }

        /// <inheritdoc />
        protected override bool SupportsDenyPermissions(IPermissionDefinitionInfo permissionDefinition)
        {
            return true;
        }

        /// <summary>Gets the DesktopModulePermissions from the Data Store.</summary>
        private void GetDesktopModulePermissions()
        {
            this.desktopModulePermissions = new DesktopModulePermissionCollection(DesktopModulePermissionController.GetDesktopModulePermissions(this.PortalDesktopModuleID));
        }

        /// <summary>Parse the Permission Keys used to persist the Permissions in the ViewState.</summary>
        /// <param name="settings">A string array of settings.</param>
        private DesktopModulePermissionInfo ParseKeys(string[] settings)
        {
            var objDesktopModulePermission = new DesktopModulePermissionInfo();

            // Call base class to load base properties
            this.ParsePermissionKeys((IPermissionInfo)objDesktopModulePermission, settings);
            if (string.IsNullOrEmpty(settings[2]))
            {
                objDesktopModulePermission.DesktopModulePermissionID = -1;
            }
            else
            {
                objDesktopModulePermission.DesktopModulePermissionID = Convert.ToInt32(settings[2], CultureInfo.InvariantCulture);
            }

            objDesktopModulePermission.PortalDesktopModuleID = this.PortalDesktopModuleID;
            return objDesktopModulePermission;
        }
    }
}
