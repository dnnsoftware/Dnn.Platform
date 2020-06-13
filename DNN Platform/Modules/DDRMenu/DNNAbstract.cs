// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu
{
    using System;
    using System.Collections.Generic;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.UI;
    using DotNetNuke.UI.WebControls;
    using DotNetNuke.Web.DDRMenu.DNNCommon;
    using DotNetNuke.Web.DDRMenu.TemplateEngine;
    using Microsoft.Extensions.DependencyInjection;

    internal static class DNNAbstract
    {
        public static string GetLoginUrl()
        {
            var request = HttpContext.Current.Request;

            if (request.IsAuthenticated)
            {
                return Globals.DependencyProvider.GetRequiredService<INavigationManager>().NavigateURL(PortalSettings.Current.ActiveTab.TabID, "Logoff");
            }

            var returnUrl = HttpContext.Current.Request.RawUrl;
            if (returnUrl.IndexOf("?returnurl=") != -1)
            {
                returnUrl = returnUrl.Substring(0, returnUrl.IndexOf("?returnurl="));
            }

            returnUrl = HttpUtility.UrlEncode(returnUrl);

            return Globals.LoginURL(returnUrl, !string.IsNullOrEmpty(request.QueryString["override"]));
        }

        public static string GetUserUrl()
        {
            var request = HttpContext.Current.Request;
            var portalSettings = PortalSettings.Current;
            if (!request.IsAuthenticated)
            {
                if (portalSettings.UserRegistration != (int)Globals.PortalRegistrationType.NoRegistration)
                {
                    return Globals.RegisterURL(HttpUtility.UrlEncode(Globals.DependencyProvider.GetRequiredService<INavigationManager>().NavigateURL()), Null.NullString);
                }
            }
            else
            {
                var objUserInfo = UserController.Instance.GetCurrentUserInfo();
                if (objUserInfo.UserID != -1)
                {
                    return Globals.UserProfileURL(objUserInfo.UserID);
                }
            }

            return string.Empty;
        }

        public static string GetCurrentCulture()
        {
            return DNNContext.Current.PortalSettings.CultureCode;
        }

        public static IEnumerable<ITemplateProcessor> SupportedTemplateProcessors()
        {
            return new ITemplateProcessor[] { new TokenTemplateProcessor(), new RazorTemplateProcessor(), new XsltTemplateProcessor() };
        }

        public static int GetNavNodeOptions(bool includeHidden)
        {
            return (int)Navigation.NavNodeOptions.IncludeSiblings + (int)Navigation.NavNodeOptions.IncludeSelf +
                   (includeHidden ? (int)Navigation.NavNodeOptions.IncludeHiddenNodes : 0);
        }

        public static bool IncludeHiddenSupported()
        {
            return true;
        }

        public static void DNNNodeToMenuNode(DNNNode dnnNode, MenuNode menuNode)
        {
            menuNode.LargeImage = dnnNode.LargeImage;
        }
    }
}
