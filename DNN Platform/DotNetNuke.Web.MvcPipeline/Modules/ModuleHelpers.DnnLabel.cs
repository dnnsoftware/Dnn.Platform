// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;
    using System.Web.UI;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Mvc;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    public static partial class ModuleHelpers
    {
        public static IHtmlString DnnLabelFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string resourceFile)
        {
            // HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes)
            var name = htmlHelper.DisplayNameFor(expression).ToString();
            if (string.IsNullOrEmpty(name))
            {
                name = htmlHelper.NameFor(expression).ToString();
            }

            name = Localization.GetString(name, resourceFile);

            var attrs = new Dictionary<string, object>();

            var div = new TagBuilder("div");
            div.AddCssClass("dnnLabel");
            div.Attributes["style"] = "position: relative;";
            var aHelp = new TagBuilder("a");
            aHelp.AddCssClass("dnnFormHelp");

            div.InnerHtml += htmlHelper.LabelFor(expression, name, attrs).ToString();
            div.InnerHtml += aHelp.ToString();

            /*
             < a id = "dnn_ctr385_ModuleSettings_plTitle_cmdHelp" tabindex = "-1" class="dnnFormHelp" aria-label="Help" href="javascript:__doPostBack(&#39;dnn$ctr385$ModuleSettings$plTitle$cmdHelp&#39;,&#39;&#39;)"></a>
                <div id = "dnn_ctr385_ModuleSettings_plTitle_pnlHelp" class="dnnTooltip">
                <div class="dnnFormHelpContent dnnClear">
                    <span id = "dnn_ctr385_ModuleSettings_plTitle_lblHelp" class="dnnHelpText">Saisissez un titre pour ce module.Il apparaitra dans la barre de titre du container utilisé pour ce module, si cette fonction est supportée par le container.</span>
                    <a href = "#" class="pinHelp" aria-label="Pin"></a>
                </div>
                </div>
            */

            return MvcHtmlString.Create(div.ToString());
        }
    }
}
