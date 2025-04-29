// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Razor;

using System;
using System.Web.WebPages;

using DotNetNuke.Internal.SourceGenerators;
using DotNetNuke.Web.Razor.Helpers;

[DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
public abstract partial class DotNetNukeWebPage : WebPageBase
{
    private dynamic model;

    public dynamic Model
    {
        get { return this.model ?? (this.model = this.PageContext.Model); }
        set { this.model = value; }
    }

    protected internal DnnHelper Dnn { get; internal set; }

    protected internal HtmlHelper Html { get; internal set; }

    protected internal UrlHelper Url { get; internal set; }

    /// <inheritdoc/>
    [DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
    protected override partial void ConfigurePage(WebPageBase parentPage)
    {
        base.ConfigurePage(parentPage);

        // Child pages need to get their context from the Parent
        this.Context = parentPage.Context;
    }
}
