// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ResourceManager.Models;

using System.Net;
using System.Text;

using DotNetNuke.Abstractions.ClientResources;

/// <summary>Represents a stylesheet resource that can be registered and rendered in the client resources controller.</summary>
public class StylesheetResource : LinkResource, IStylesheetResource
{
    private readonly IClientResourceController clientResourceController;

    /// <summary>Initializes a new instance of the <see cref="StylesheetResource"/> class.</summary>
    /// <param name="clientResourceController">The client resources controller used to register the stylesheet.</param>
    public StylesheetResource(IClientResourceController clientResourceController)
    {
        this.clientResourceController = clientResourceController;
        this.Provider = ClientResourceProviders.DefaultCssProvider;
        this.Priority = (int)FileOrder.Css.DefaultPriority;
    }

    /// <inheritdoc />
    public bool Disabled { get; set; } = false;

    /// <inheritdoc />
    public override void Register()
    {
        this.clientResourceController.AddStylesheet(this);
    }

    /// <inheritdoc />
    public override string Render(int crmVersion, bool useCdn, string applicationPath)
    {
        var htmlString = new StringBuilder("<link");
        htmlString.Append($" href=\"{WebUtility.HtmlEncode(this.GetVersionedPath(crmVersion, useCdn, applicationPath))}\"");
        if (this.Preload)
        {
            htmlString.Append($" rel=\"preload\" as=\"style\"");
        }
        else
        {
            htmlString.Append($" rel=\"stylesheet\"");
        }

        if (this.Disabled)
        {
            htmlString.Append(" disabled");
        }

        this.RenderMedia(htmlString);
        this.RenderType(htmlString);
        this.RenderBlocking(htmlString);
        this.RenderCrossOriginAttribute(htmlString);
        this.RenderFetchPriority(htmlString);
        this.RenderIntegrity(htmlString);
        this.RenderReferrerPolicy(htmlString);
        this.RenderAttributes(htmlString);
        htmlString.Append(" />");
        return htmlString.ToString();
    }
}
