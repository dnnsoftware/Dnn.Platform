// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions;

using System;
using System.Collections;
using System.Collections.Generic;

using DotNetNuke.Common.Utilities;

/// Project  : DotNetNuke
/// Namespace: DotNetNuke.Security.Permissions
/// Class    : DesktopModulePermissionCollection
/// <summary>
/// DesktopModulePermissionCollection provides the a custom collection for DesktopModulePermissionInfo
/// objects.
/// </summary>
[Serializable]
public class DesktopModulePermissionCollection : CollectionBase
{
    /// <summary>Initializes a new instance of the <see cref="DesktopModulePermissionCollection"/> class.</summary>
    public DesktopModulePermissionCollection()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="DesktopModulePermissionCollection"/> class.</summary>
    /// <param name="desktopModulePermissions"></param>
    public DesktopModulePermissionCollection(ArrayList desktopModulePermissions)
    {
        this.AddRange(desktopModulePermissions);
    }

    /// <summary>Initializes a new instance of the <see cref="DesktopModulePermissionCollection"/> class.</summary>
    /// <param name="desktopModulePermissions"></param>
    public DesktopModulePermissionCollection(DesktopModulePermissionCollection desktopModulePermissions)
    {
        this.AddRange(desktopModulePermissions);
    }

    /// <summary>Initializes a new instance of the <see cref="DesktopModulePermissionCollection"/> class.</summary>
    /// <param name="desktopModulePermissions"></param>
    /// <param name="desktopModulePermissionID"></param>
    public DesktopModulePermissionCollection(ArrayList desktopModulePermissions, int desktopModulePermissionID)
    {
        foreach (DesktopModulePermissionInfo permission in desktopModulePermissions)
        {
            if (permission.DesktopModulePermissionID == desktopModulePermissionID)
            {
                this.Add(permission);
            }
        }
    }

    public DesktopModulePermissionInfo this[int index]
    {
        get
        {
            return (DesktopModulePermissionInfo)this.List[index];
        }

        set
        {
            this.List[index] = value;
        }
    }

    public int Add(DesktopModulePermissionInfo value)
    {
        return this.List.Add(value);
    }

    public int Add(DesktopModulePermissionInfo value, bool checkForDuplicates)
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

    public void AddRange(ArrayList desktopModulePermissions)
    {
        foreach (DesktopModulePermissionInfo permission in desktopModulePermissions)
        {
            this.Add(permission);
        }
    }

    public void AddRange(DesktopModulePermissionCollection desktopModulePermissions)
    {
        foreach (DesktopModulePermissionInfo permission in desktopModulePermissions)
        {
            this.Add(permission);
        }
    }

    public bool CompareTo(DesktopModulePermissionCollection objDesktopModulePermissionCollection)
    {
        if (objDesktopModulePermissionCollection.Count != this.Count)
        {
            return false;
        }

        this.InnerList.Sort(new CompareDesktopModulePermissions());
        objDesktopModulePermissionCollection.InnerList.Sort(new CompareDesktopModulePermissions());
        for (int i = 0; i <= this.Count - 1; i++)
        {
            if (objDesktopModulePermissionCollection[i].DesktopModulePermissionID != this[i].DesktopModulePermissionID || objDesktopModulePermissionCollection[i].AllowAccess != this[i].AllowAccess)
            {
                return false;
            }
        }

        return true;
    }

    public bool Contains(DesktopModulePermissionInfo value)
    {
        return this.List.Contains(value);
    }

    public int IndexOf(DesktopModulePermissionInfo value)
    {
        return this.List.IndexOf(value);
    }

    public void Insert(int index, DesktopModulePermissionInfo value)
    {
        this.List.Insert(index, value);
    }

    public void Remove(DesktopModulePermissionInfo value)
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
}
