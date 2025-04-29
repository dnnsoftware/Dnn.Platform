// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users;

using System;

using DotNetNuke.Internal.SourceGenerators;

/// Project:    DotNetNuke
/// Namespace:  DotNetNuke.Entities.Users
/// Class:      BaseUserInfo
/// <summary>The BaseUserInfo class provides a base Entity for an online user.</summary>
[Serializable]
[DnnDeprecated(8, 0, 0, "Other solutions exist outside of the DNN Platform", RemovalVersion = 11)]
public abstract partial class BaseUserInfo
{
    private DateTime creationDate;
    private DateTime lastActiveDate;
    private int portalID;
    private int tabID;

    /// <summary>Gets or sets the PortalId for this online user.</summary>
    public int PortalID
    {
        get
        {
            return this.portalID;
        }

        set
        {
            this.portalID = value;
        }
    }

    /// <summary>Gets or sets the TabId for this online user.</summary>
    public int TabID
    {
        get
        {
            return this.tabID;
        }

        set
        {
            this.tabID = value;
        }
    }

    /// <summary>Gets or sets the CreationDate for this online user.</summary>
    public DateTime CreationDate
    {
        get
        {
            return this.creationDate;
        }

        set
        {
            this.creationDate = value;
        }
    }

    /// <summary>Gets or sets the LastActiveDate for this online user.</summary>
    public DateTime LastActiveDate
    {
        get
        {
            return this.lastActiveDate;
        }

        set
        {
            this.lastActiveDate = value;
        }
    }
}
