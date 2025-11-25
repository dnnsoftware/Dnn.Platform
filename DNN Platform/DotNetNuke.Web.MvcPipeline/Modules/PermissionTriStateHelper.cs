// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Modules
{
    using System;
    using System.Collections;
    using System.Web;
    using System.Web.Mvc;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Entities.Icons;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Client.ResourceManager;
    using DotNetNuke.Web.MvcPipeline.Framework.JavascriptLibraries;
    using DotNetNuke.Web.MvcPipeline.UI.Utilities;
    using Microsoft.Extensions.DependencyInjection;

    public static class PermissionTriStateHelper
    {
        public static MvcHtmlString PermissionTriState(
            this HtmlHelper helper,
            string name,
            string value,
            bool isFullControl = false,
            bool isView = false,
            bool locked = false,
            bool supportsDenyMode = true,
            string permissionKey = "")
        {
            const string scriptKey = "initTriState";
            JavaScript.RequestRegistration(CommonJs.jQuery);
            var controller = GetClientResourcesController();
            controller.RegisterScript("/js/dnn.permissiontristate.js");
            MvcClientAPI.RegisterStartupScript(scriptKey, GetInitScript());

            var grantImagePath = IconController.IconURL("Grant");
            var denyImagePath = IconController.IconURL("Deny");
            var nullImagePath = IconController.IconURL("Unchecked");
            var lockImagePath = IconController.IconURL("Lock");

            var grantAltText = Localization.GetString("PermissionTypeGrant");
            var denyAltText = Localization.GetString("PermissionTypeDeny");
            var nullAltText = Localization.GetString("PermissionTypeNull");

            string imagePath;
            string altText;
            switch (value)
            {
                case "True":
                    imagePath = grantImagePath;
                    altText = grantAltText;
                    break;
                case "False":
                    imagePath = denyImagePath;
                    altText = denyAltText;
                    break;
                default:
                    imagePath = nullImagePath;
                    altText = nullAltText;
                    break;
            }

            var cssClass = "tristate";
            if (locked)
            {
                imagePath = lockImagePath;
                cssClass += " lockedPerm";
            }

            if (!supportsDenyMode)
            {
                cssClass += " noDenyPerm";
            }

            if (isFullControl)
            {
                cssClass += " fullControl";
            }

            if (isView && !locked)
            {
                cssClass += " view";
            }

            if (!string.IsNullOrEmpty(permissionKey) && !isView && !isFullControl)
            {
                cssClass += " " + permissionKey.ToLowerInvariant();
            }

            var img = new TagBuilder("img");
            img.MergeAttribute("src", imagePath);
            img.MergeAttribute("alt", altText);

            var hidden = new TagBuilder("input");
            hidden.MergeAttribute("type", "hidden");
            hidden.MergeAttribute("name", name);
            hidden.MergeAttribute("value", value);
            hidden.MergeAttribute("class", cssClass);
            hidden.MergeAttribute("id", name);

            return MvcHtmlString.Create(img.ToString(TagRenderMode.SelfClosing) + hidden.ToString(TagRenderMode.SelfClosing));
        }

        public static string GetInitScript()
        {
            string grantImagePath, denyImagePath, nullImagePath, lockImagePath, grantAltText, denyAltText, nullAltText;

            LookupScriptValues(out grantImagePath, out denyImagePath, out nullImagePath, out lockImagePath, out grantAltText, out denyAltText, out nullAltText);

            var script =
                    string.Format(
                        @"jQuery(document).ready(
                            function() {{
                                var images = {{ 'True': '{0}', 'False': '{1}', 'Null': '{2}' }};
                                var toolTips = {{ 'True': '{3}', 'False': '{4}', 'Null': '{5}' }};
                                var tsm = dnn.controls.triStateManager(images, toolTips);
                                jQuery('.tristate').each( function(i, elem) {{
                                  tsm.initControl( elem );
                                }});
                             }});",
                        grantImagePath,
                        denyImagePath,
                        nullImagePath,
                        grantAltText,
                        denyAltText,
                        nullAltText);

            return script;
        }

        private static void LookupScriptValues(out string grantImagePath, out string denyImagePath, out string nullImagePath, out string lockImagePath, out string grantAltText, out string denyAltText, out string nullAltText)
        {
            grantImagePath = IconController.IconURL("Grant");
            denyImagePath = IconController.IconURL("Deny");
            nullImagePath = IconController.IconURL("Unchecked");
            lockImagePath = IconController.IconURL("Lock");

            grantAltText = Localization.GetString("PermissionTypeGrant");
            denyAltText = Localization.GetString("PermissionTypeDeny");
            nullAltText = Localization.GetString("PermissionTypeNull");
        }

        private static IClientResourceController GetClientResourcesController()
        {
            var serviceProvider = DotNetNuke.Common.Globals.GetCurrentServiceProvider();
            return serviceProvider.GetRequiredService<IClientResourceController>();
        }
    }
}
