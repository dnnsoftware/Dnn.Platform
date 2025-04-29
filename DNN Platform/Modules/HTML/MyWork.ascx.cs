// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Html;

using System;

using DotNetNuke.Abstractions;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using Microsoft.Extensions.DependencyInjection;

/// <summary>  MyWork allows a user to view any outstanding workflow items.</summary>
public partial class MyWork : PortalModuleBase
{
    private readonly INavigationManager navigationManager;

    /// <summary>Initializes a new instance of the <see cref="MyWork"/> class.</summary>
    public MyWork()
    {
        this.navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
    }

    public string FormatURL(object dataItem)
    {
        var objHtmlTextUser = (HtmlTextUserInfo)dataItem;
        return "<a href=\"" + this.navigationManager.NavigateURL(objHtmlTextUser.TabID) + "#" + objHtmlTextUser.ModuleID + "\">" + objHtmlTextUser.ModuleTitle + " ( " + objHtmlTextUser.StateName + " )</a>";
    }

    /// <summary>  Page_Load runs when the control is loaded.</summary>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        this.hlCancel.NavigateUrl = this.navigationManager.NavigateURL();

        try
        {
            if (!this.Page.IsPostBack)
            {
                var objHtmlTextUsers = new HtmlTextUserController();
                this.dgTabs.DataSource = objHtmlTextUsers.GetHtmlTextUser(this.UserInfo.UserID);
                this.dgTabs.DataBind();
            }
        }
        catch (Exception exc)
        {
            Exceptions.ProcessModuleLoadException(this, exc);
        }
    }
}
