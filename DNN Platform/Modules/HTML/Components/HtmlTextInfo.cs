// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html;

using System;

using DotNetNuke.Entities;

/// Namespace:  DotNetNuke.Modules.Html
/// Project:    DotNetNuke
/// Class:      HtmlTextInfo
/// <summary>  Defines an instance of an HtmlText object.</summary>
[Serializable]
public class HtmlTextInfo : BaseEntityInfo
{
    // local property declarations
    private bool approved = true;
    private string comment = string.Empty;
    private bool isActive = true;
    private int itemID = -1;

    // initialization

    // public properties
    public int ItemID
    {
        get
        {
            return this.itemID;
        }

        set
        {
            this.itemID = value;
        }
    }

    public int ModuleID { get; set; }

    public string Content { get; set; }

    public int Version { get; set; }

    public int WorkflowID { get; set; }

    public string WorkflowName { get; set; }

    public int StateID { get; set; }

    public string StateName { get; set; }

    public bool IsPublished { get; set; }

    public int PortalID { get; set; }

    public bool Notify { get; set; }

    public bool IsActive
    {
        get
        {
            return this.isActive;
        }

        set
        {
            this.isActive = value;
        }
    }

    public string Comment
    {
        get
        {
            return this.comment;
        }

        set
        {
            this.comment = value;
        }
    }

    public bool Approved
    {
        get
        {
            return this.approved;
        }

        set
        {
            this.approved = value;
        }
    }

    public string DisplayName { get; set; }

    public string Summary { get; set; }
}
