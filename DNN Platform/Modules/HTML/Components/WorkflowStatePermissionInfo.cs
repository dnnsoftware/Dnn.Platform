#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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