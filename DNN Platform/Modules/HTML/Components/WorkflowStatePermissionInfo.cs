// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Permissions
{
    using System;
    using System.Data;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class    : DesktopModulePermissionInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   DesktopModulePermissionInfo provides the Entity Layer for DesktopModulePermissionInfo
    ///   Permissions.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class WorkflowStatePermissionInfo : PermissionInfoBase, IHydratable
    {
        // local property declarations
        private int _StateID;
        private int _WorkflowStatePermissionID;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowStatePermissionInfo"/> class.
        ///   Constructs a new WorkflowStatePermissionInfo.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public WorkflowStatePermissionInfo()
        {
            this._WorkflowStatePermissionID = Null.NullInteger;
            this._StateID = Null.NullInteger;
        }

        // New

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowStatePermissionInfo"/> class.
        ///   Constructs a new WorkflowStatePermissionInfo.
        /// </summary>
        /// <param name = "permission">A PermissionInfo object.</param>
        /// -----------------------------------------------------------------------------
        public WorkflowStatePermissionInfo(PermissionInfo permission)
            : this()
        {
            this.ModuleDefID = permission.ModuleDefID;
            this.PermissionCode = permission.PermissionCode;
            this.PermissionID = permission.PermissionID;
            this.PermissionKey = permission.PermissionKey;
            this.PermissionName = permission.PermissionName;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets or sets and sets the WorkflowState Permission ID.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        public int WorkflowStatePermissionID
        {
            get
            {
                return this._WorkflowStatePermissionID;
            }

            set
            {
                this._WorkflowStatePermissionID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets or sets and sets the State ID.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        public int StateID
        {
            get
            {
                return this._StateID;
            }

            set
            {
                this._StateID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets or sets and sets the Key ID.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        public int KeyID
        {
            get
            {
                return this.WorkflowStatePermissionID;
            }

            set
            {
                this.WorkflowStatePermissionID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Compares if two WorkflowStatePermissionInfo objects are equivalent/equal.
        /// </summary>
        /// <param name = "obj">a WorkflowStatePermissionObject.</param>
        /// <returns>true if the permissions being passed represents the same permission
        ///   in the current object.
        /// </returns>
        /// <remarks>
        ///   This function is needed to prevent adding duplicates to the WorkflowStatePermissionCollection.
        ///   WorkflowStatePermissionCollection.Contains will use this method to check if a given permission
        ///   is already included in the collection.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != typeof(WorkflowStatePermissionInfo))
            {
                return false;
            }

            return this.Equals((WorkflowStatePermissionInfo)obj);
        }

        public bool Equals(WorkflowStatePermissionInfo other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return (this.AllowAccess == other.AllowAccess) && (this.StateID == other.StateID) && (this.RoleID == other.RoleID) && (this.PermissionID == other.PermissionID);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this._StateID * 397) ^ this._WorkflowStatePermissionID;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Fills a WorkflowStatePermissionInfo from a Data Reader.
        /// </summary>
        /// <param name = "dr">The Data Reader to use.</param>
        /// -----------------------------------------------------------------------------
        public void Fill(IDataReader dr)
        {
            // Call the base classes fill method to populate base class proeprties
            this.FillInternal(dr);

            this.WorkflowStatePermissionID = Null.SetNullInteger(dr["WorkflowStatePermissionID"]);
            this.StateID = Null.SetNullInteger(dr["StateID"]);
        }
    }
}
