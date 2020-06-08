// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System.Web.UI;

namespace DotNetNuke.Services.Tokens
{
    public class HtmlTokenReplace : TokenReplace
    {
        public HtmlTokenReplace(Page page)
            : base(Scope.DefaultSettings)
        {
            PropertySource["css"] = new CssPropertyAccess(page);
            PropertySource["js"] = new JavaScriptPropertyAccess(page);
            PropertySource["javascript"] = new JavaScriptPropertyAccess(page);
            PropertySource["antiforgerytoken"] = new AntiForgeryTokenPropertyAccess();
        }
    }
}
