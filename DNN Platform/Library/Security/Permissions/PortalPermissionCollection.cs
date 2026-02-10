// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;

    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Collections;
    using DotNetNuke.Common.Utilities;

    /// <summary>PortalPermissionCollection provides a custom collection for <see cref="PortalPermissionInfo"/> objects.</summary>
    [Serializable]
    [XmlRoot("portalpermissions")]
    public class PortalPermissionCollection : GenericCollectionBase<PortalPermissionInfo>
    {
        /// <summary>Initializes a new instance of the <see cref="PortalPermissionCollection"/> class.</summary>
        public PortalPermissionCollection()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PortalPermissionCollection"/> class.</summary>
        /// <param name="portalPermissions">An <see cref="ArrayList"/> of <see cref="PortalPermissionInfo"/> instances.</param>
        public PortalPermissionCollection(ArrayList portalPermissions)
        {
            this.AddRange(portalPermissions);
        }

        /// <summary>Initializes a new instance of the <see cref="PortalPermissionCollection"/> class.</summary>
        /// <param name="portalPermissions">A collection of <see cref="PortalPermissionInfo"/> instances.</param>
        public PortalPermissionCollection(PortalPermissionCollection portalPermissions)
        {
            this.AddRange(portalPermissions);
        }

        /// <summary>Initializes a new instance of the <see cref="PortalPermissionCollection"/> class.</summary>
        /// <param name="portalPermissions">An <see cref="ArrayList"/> of <see cref="PortalPermissionInfo"/> instances.</param>
        /// <param name="portalId">The ID of the portal by which to filter <paramref name="portalPermissions"/>.</param>
        public PortalPermissionCollection(ArrayList portalPermissions, int portalId)
        {
            foreach (PortalPermissionInfo permission in portalPermissions)
            {
                if (permission.PortalID == portalId)
                {
                    this.Add(permission);
                }
            }
        }

        public int Add(PortalPermissionInfo value, bool checkForDuplicates)
        {
            int id = Null.NullInteger;

            if (!checkForDuplicates)
            {
                id = this.Add(value);
            }
            else
            {
                bool isMatch = false;
                foreach (IPermissionInfo permission in this.List)
                {
                    if (permission.PermissionId == value.PermissionID && permission.UserId == value.UserID && permission.RoleId == value.RoleID)
                    {
                        isMatch = true;
                        break;
                    }
                }

                if (!isMatch)
                {
                    id = this.Add(value);
                }
            }

            return id;
        }

        public void AddRange(ArrayList portalPermissions)
        {
            foreach (PortalPermissionInfo permission in portalPermissions)
            {
                this.Add(permission);
            }
        }

        public void AddRange(IEnumerable<PortalPermissionInfo> portalPermissions)
        {
            foreach (PortalPermissionInfo permission in portalPermissions)
            {
                this.Add(permission);
            }
        }

        public void AddRange(PortalPermissionCollection portalPermissions)
        {
            foreach (PortalPermissionInfo permission in portalPermissions)
            {
                this.Add(permission);
            }
        }

        public bool CompareTo(PortalPermissionCollection objPortalPermissionCollection)
        {
            if (objPortalPermissionCollection.Count != this.Count)
            {
                return false;
            }

            this.InnerList.Sort(new ComparePortalPermissions());
            objPortalPermissionCollection.InnerList.Sort(new ComparePortalPermissions());
            for (int i = 0; i <= this.Count - 1; i++)
            {
                if (objPortalPermissionCollection[i].PortalPermissionID != this[i].PortalPermissionID
                        || objPortalPermissionCollection[i].PermissionID != this[i].PermissionID
                        || objPortalPermissionCollection[i].RoleID != this[i].RoleID
                        || objPortalPermissionCollection[i].UserID != this[i].UserID
                        || objPortalPermissionCollection[i].AllowAccess != this[i].AllowAccess)
                {
                    return false;
                }
            }

            return true;
        }

        public void Remove(int permissionID, int roleID, int userID)
        {
            foreach (PermissionInfoBase permission in this.List)
            {
                if (permission.PermissionID == permissionID && permission.UserID == userID && permission.RoleID == roleID)
                {
                    this.List.Remove(permission);
                    break;
                }
            }
        }

        public List<PermissionInfoBase> ToList()
        {
            return [..this.List.Cast<PermissionInfoBase>()];
        }

        public string ToString(string key)
        {
            return PermissionController.BuildPermissions(this.List, key);
        }

        public IEnumerable<PortalPermissionInfo> Where(Func<PortalPermissionInfo, bool> predicate)
        {
            return this.Cast<PortalPermissionInfo>().Where(predicate);
        }
    }
}
