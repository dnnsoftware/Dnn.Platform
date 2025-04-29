// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins;

using System;

/// Project  : DotNetNuke
/// Class    : SkinInfo
///
/// <summary>    Handles the Business Object for Skins.</summary>
[Serializable]
public class SkinInfo
{
    private int portalId;
    private int skinId;
    private string skinRoot;
    private string skinSrc;
    private SkinType skinType;

    public int SkinId
    {
        get
        {
            return this.skinId;
        }

        set
        {
            this.skinId = value;
        }
    }

    public int PortalId
    {
        get
        {
            return this.portalId;
        }

        set
        {
            this.portalId = value;
        }
    }

    public string SkinRoot
    {
        get
        {
            return this.skinRoot;
        }

        set
        {
            this.skinRoot = value;
        }
    }

    public SkinType SkinType
    {
        get
        {
            return this.skinType;
        }

        set
        {
            this.skinType = value;
        }
    }

    public string SkinSrc
    {
        get
        {
            return this.skinSrc;
        }

        set
        {
            this.skinSrc = value;
        }
    }
}
