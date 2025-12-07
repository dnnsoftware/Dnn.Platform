// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNNConnect.CKEditorProvider.Web
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq.Expressions;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;
    using System.Web.UI.WebControls;

    using DNNConnect.CKEditorProvider.Utilities;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Web.MvcPipeline.Controllers;
    using DotNetNuke.Web.MvcPipeline.UI.Utilities;
    using Microsoft.Extensions.DependencyInjection;

    public static class CKEditorHelper
    {
        public static IHtmlString CKEditorEditorFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, int moduleId)
        {
            var controller = htmlHelper.ViewContext.Controller as DnnPageController;

            if (controller == null)
            {
                throw new InvalidOperationException("The DnnHelper class can only be used from DnnPageController");
            }

            // HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes)
            var id = htmlHelper.IdFor(expression);

            var attrs = new Dictionary<string, object>();
            attrs.Add("id", id.ToString());
            attrs.Add("data-ckeditor", true);

            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            if (!htmlHelper.ViewContext.HttpContext.Request.IsAjaxRequest())
            {
                var cdf = controller.DependencyProvider.GetRequiredService<IClientResourceController>();

                cdf.RegisterStylesheet("~/Providers/HtmlEditorProviders/DNNConnect.CKE/css/CKEditorToolBars.css");
                cdf.RegisterStylesheet("~/Providers/HtmlEditorProviders/DNNConnect.CKE/css/CKEditorOverride.css");

                // controller.RegisterStylesheet("~/Providers/HtmlEditorProviders/DNNConnect.CKE/js/ckeditor/4.18.0/editor.css");
                /*
                const string CsName = "CKEdScript";
                const string CsFindName = "CKFindScript";
                */

                JavaScript.RequestRegistration(CommonJs.jQuery);

                cdf.RegisterScript("~/Providers/HtmlEditorProviders/DNNConnect.CKE/js/ckeditor/4.18.0/ckeditor.js");
                cdf.RegisterScript("~/Providers/HtmlEditorProviders/DNNConnect.CKE/js/editorOverride.js");

                LoadEditorSettings(portalSettings, portalSettings.ActiveTab.TabID, moduleId);
            }

            return htmlHelper.TextAreaFor(expression, attrs);
        }

        private static void LoadEditorSettings(PortalSettings portalSettings, int tabId, int moduleId)
        {
            const string ProviderType = "htmlEditor";

            // Load config settings
            var settings = SettingsLoader.LoadConfigSettings(ProviderType);
            var configFolder = settings["configFolder"];

            // Load editor settings
            var currentEditorSettings = SettingsLoader.LoadSettings(
                portalSettings,
                moduleId,
                moduleId.ToString(),
                configFolder);

            // Get module configuration
            var moduleConfiguration = ModuleController.Instance.GetModule(moduleId, tabId, false);

            // Populate settings
            var emptyAttributes = new NameValueCollection();
            SettingsLoader.PopulateSettings(
                settings,
                currentEditorSettings,
                portalSettings,
                moduleConfiguration,
                emptyAttributes,
                Unit.Empty,
                Unit.Empty,
                moduleId.ToString(),
                moduleId,
                null);

            // Generate config script
            var editorVar = $"editor{moduleId}";
            var configScript = SettingsLoader.GetEditorConfigScript(settings, editorVar);
            MvcClientAPI.RegisterStartupScript("CKEditorConfig", configScript);
        }
    }
}
