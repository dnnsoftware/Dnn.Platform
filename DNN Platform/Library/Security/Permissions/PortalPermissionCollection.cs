// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;

/// Project  : DotNetNuke
/// Namespace: DotNetNuke.Security.Permissions
/// Class    : PortalPermissionCollection
/// <summary>
/// PortalPermissionCollection provides the a custom collection for PortalPermissionInfo
/// objects.
/// </summary>
[Serializable]
[XmlRoot("portalpermissions")]
public class PortalPermissionCollection : CollectionBase
{
    /// <summary>Initializes a new instance of the <see cref="PortalPermissionCollection"/> class.</summary>
    public PortalPermissionCollection()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="PortalPermissionCollection"/> class.</summary>
    /// <param name="portalPermissions"></param>
    public PortalPermissionCollection(ArrayList portalPermissions)
    {
        this.AddRange(portalPermissions);
    }

    /// <summary>Initializes a new instance of the <see cref="PortalPermissionCollection"/> class.</summary>
    /// <param name="portalPermissions"></param>
    public PortalPermissionCollection(PortalPermissionCollection portalPermissions)
    {
        this.AddRange(portalPermissions);
    }

    /// <summary>Initializes a new instance of the <see cref="PortalPermissionCollection"/> class.</summary>
    /// <param name="portalPermissions"></param>
    /// <param name="portalId"></param>
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

    public PortalPermissionInfo this[int index]
    {
        get
        {
            return (PortalPermissionInfo)this.List[index];
        }

        set
        {
            this.List[index] = value;
        }
    }

    public int Add(PortalPermissionInfo value)
    {
        return this.List.Add(value);
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
            foreach (PermissionInfoBase permission in this.List)
            {
                if (permission.PermissionID == value.PermissionID && permission.UserID == value.UserID && permission.RoleID == value.RoleID)
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

    public bool Contains(PortalPermissionInfo value)
    {
        return this.List.Contains(value);
    }

    public int IndexOf(PortalPermissionInfo value)
    {
        return this.List.IndexOf(value);
    }

    public void Insert(int index, PortalPermissionInfo value)
    {
        this.List.Insert(index, value);
    }

    public void Remove(PortalPermissionInfo value)
    {
        this.List.Remove(value);
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
        var list = new List<PermissionInfoBase>();
        foreach (PermissionInfoBase permission in this.List)
        {
            list.Add(permission);
        }

        return list;
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
