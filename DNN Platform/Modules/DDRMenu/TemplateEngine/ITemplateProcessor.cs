// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Web.UI;

namespace DotNetNuke.Web.DDRMenu.TemplateEngine
{
    internal interface ITemplateProcessor
    {
        bool LoadDefinition(TemplateDefinition baseDefinition);
        void Render(object source, HtmlTextWriter htmlWriter, TemplateDefinition liveDefinition);
    }
}
