// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Tokens
{
    using System.Web.UI;

    public class HtmlTokenReplace : TokenReplace
    {
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
