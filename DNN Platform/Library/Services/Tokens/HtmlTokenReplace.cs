﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Tokens
{
    using System.Web.UI;

    public class HtmlTokenReplace : TokenReplace
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlTokenReplace"/> class.
        /// </summary>
        /// <param name="page"></param>
        public HtmlTokenReplace(Page page)
            : base(Scope.DefaultSettings)
        {
            this.PropertySource["css"] = new CssPropertyAccess(page);
            this.PropertySource["js"] = new JavaScriptPropertyAccess(page);
            this.PropertySource["javascript"] = new JavaScriptPropertyAccess(page);
            this.PropertySource["antiforgerytoken"] = new AntiForgeryTokenPropertyAccess();
        }
    }
}
