// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Analytics.Config;

using System;

[Serializable]
public class AnalyticsRule
{
    private string label;
    private int roleId;
    private int tabId;

    public int RoleId
    {
        get
        {
            return this.roleId;
        }

        set
        {
            this.roleId = value;
        }
    }

    public int TabId
    {
        get
        {
            return this.tabId;
        }

        set
        {
            this.tabId = value;
        }
    }

    public string Label
    {
        get
        {
            return this.label;
        }

        set
        {
            this.label = value;
        }
    }

    public string Value { get; set; }
}
