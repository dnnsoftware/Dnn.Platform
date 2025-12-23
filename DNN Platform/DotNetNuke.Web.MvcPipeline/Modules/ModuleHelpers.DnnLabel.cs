// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    using DotNetNuke.Services.Localization;
    
    /// <summary>
    /// HTML helper extensions for rendering DNN-style labels with help icons.
    /// </summary>
    public static partial class ModuleHelpers
    {
        /// <summary>
        /// Renders a DNN label with help icon for the specified model expression, using the given resource file for localization.
        /// </summary>
        /// <typeparam name="TModel">The model type.</typeparam>
        /// <typeparam name="TProperty">The property type.</typeparam>
        /// <param name="htmlHelper">The strongly-typed HTML helper.</param>
        /// <param name="expression">The expression identifying the field.</param>
        /// <param name="resourceFile">The localization resource file used to resolve the label text.</param>
        /// <returns>The rendered label HTML.</returns>
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
