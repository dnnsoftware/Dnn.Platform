// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Services.Tokens;

using System;
using System.Web.UI;

using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;

public class CssPropertyAccess : JsonPropertyAccess<StylesheetDto>
{
    private readonly Page page;

    /// <summary>Initializes a new instance of the <see cref="CssPropertyAccess"/> class.</summary>
    /// <param name="page"></param>
    public CssPropertyAccess(Page page)
    {
        this.page = page;
    }

    /// <inheritdoc/>
    protected override string ProcessToken(StylesheetDto model, UserInfo accessingUser, Scope accessLevel)
    {
        if (string.IsNullOrEmpty(model.Path))
        {
            throw new ArgumentException("The Css token must specify a path or property.");
        }

        if (model.Priority == 0)
        {
            model.Priority = (int)FileOrder.Css.DefaultPriority;
        }

        if (string.IsNullOrEmpty(model.Provider))
        {
            ClientResourceManager.RegisterStyleSheet(this.page, model.Path, model.Priority);
        }
        else
        {
            ClientResourceManager.RegisterStyleSheet(this.page, model.Path, model.Priority, model.Provider);
        }

        return string.Empty;
    }
}
