// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Data;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;


namespace DotNetNuke.Security.Permissions
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class	 : DesktopModulePermissionInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   DesktopModulePermissionInfo provides the Entity Layer for DesktopModulePermissionInfo 
    ///   Permissions
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class WorkflowStatePermissionInfo : PermissionInfoBase, IHydratable
    {
        // local property declarations

        private int _StateID;
        private int _WorkflowStatePermissionID;

        #region Constructors

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Constructs a new WorkflowStatePermissionInfo
        /// </summary>
        /// -----------------------------------------------------------------------------
        public WorkflowStatePermissionInfo()
        {
            _WorkflowStatePermissionID = Null.NullInteger;
            _StateID = Null.NullInteger;
        }

        //New

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Constructs a new WorkflowStatePermissionInfo
        /// </summary>
        /// <param name = "permission">A PermissionInfo object</param>
        /// -----------------------------------------------------------------------------
        public WorkflowStatePermissionInfo(PermissionInfo permission) : this()
        {
            ModuleDefID = permission.ModuleDefID;
            PermissionCode = permission.PermissionCode;
            PermissionID = permission.PermissionID;
            PermissionKey = permission.PermissionKey;
            PermissionName = permission.PermissionName;
        }

        #endregion

        #region Public Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets and sets the WorkflowState Permission ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        public int WorkflowStatePermissionID
        {
            get
            {
                return _WorkflowStatePermissionID;
            }
            set
            {
                _WorkflowStatePermissionID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets and sets the State ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        public int StateID
        {
            get
            {
                return _StateID;
            }
            set
            {
                _StateID = value;
            }
        }

        #endregion

        #region Public Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Compares if two WorkflowStatePermissionInfo objects are equivalent/equal
        /// </summary>
        /// <param name = "obj">a WorkflowStatePermissionObject</param>
        /// <returns>true if the permissions being passed represents the same permission
        ///   in the current object
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
            return Equals((WorkflowStatePermissionInfo)obj);
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
            return (AllowAccess == other.AllowAccess) && (StateID == other.StateID) && (RoleID == other.RoleID) && (PermissionID == other.PermissionID);
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                return (_StateID*397) ^ _WorkflowStatePermissionID;
            }
        }

        #endregion

        #region IHydratable Implementation

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Fills a WorkflowStatePermissionInfo from a Data Reader
        /// </summary>
        /// <param name = "dr">The Data Reader to use</param>
        /// -----------------------------------------------------------------------------
        public void Fill(IDataReader dr)
        {
            //Call the base classes fill method to populate base class proeprties
            base.FillInternal(dr);

            WorkflowStatePermissionID = Null.SetNullInteger(dr["WorkflowStatePermissionID"]);
            StateID = Null.SetNullInteger(dr["StateID"]);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets and sets the Key ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        public int KeyID
        {
            get
            {
                return WorkflowStatePermissionID;
            }
            set
            {
                WorkflowStatePermissionID = value;
            }
        }

        #endregion
    }
}
