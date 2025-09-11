// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Tokens
{
    using System.Web.Mvc;

    using DotNetNuke.Services.Tokens;
    using DotNetNuke.UI.Modules.Html5;

    /// <summary>A <see cref="TokenReplace"/> for MVC HTML modules.</summary>
    public class MvcHtmlTokenReplace : TokenReplace
    {
        /// <summary>Initializes a new instance of the <see cref="MvcHtmlTokenReplace"/> class.</summary>
        /// <param name="controllerContext">The controller context in which the module is rendering.</param>
        public MvcHtmlTokenReplace(ControllerContext controllerContext)
            : base(Scope.DefaultSettings)
        {
            this.AddPropertySource("css", new MvcCssPropertyAccess(controllerContext));
            this.AddPropertySource("js", new MvcJavaScriptPropertyAccess(controllerContext));
            this.AddPropertySource("javascript", new MvcJavaScriptPropertyAccess(controllerContext));
            this.AddPropertySource("antiforgerytoken", new AntiForgeryTokenPropertyAccess());
        }
    }
}
