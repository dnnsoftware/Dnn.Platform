// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Tokens
{
    using System;
    using System.Web.UI;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Common;
    using Microsoft.Extensions.DependencyInjection;

    public class HtmlTokenReplace : TokenReplace
    {
        /// <summary>Initializes a new instance of the <see cref="HtmlTokenReplace"/> class.</summary>
        /// <param name="page">The page on which the module is rendering.</param>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IClientResourceController. Scheduled removal in v12.0.0.")]
        public HtmlTokenReplace(Page page)
            : this((IClientResourceController)null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="HtmlTokenReplace"/> class.</summary>
        /// <param name="clientResourceController">The client resource controller.</param>
        public HtmlTokenReplace(IClientResourceController clientResourceController)
            : base(Scope.DefaultSettings)
        {
            clientResourceController ??= Globals.DependencyProvider.GetRequiredService<IClientResourceController>();

            this.AddPropertySource("css", new CssPropertyAccess(clientResourceController));
            this.AddPropertySource("js", new JavaScriptPropertyAccess(clientResourceController));
            this.AddPropertySource("javascript", new JavaScriptPropertyAccess(clientResourceController));
            this.AddPropertySource("antiforgerytoken", new AntiForgeryTokenPropertyAccess());
        }
    }
}
