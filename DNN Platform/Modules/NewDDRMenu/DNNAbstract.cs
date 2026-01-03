// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.NewDDRMenu
{
    using System.Collections.Generic;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.UI;
    using DotNetNuke.UI.WebControls;
    using DotNetNuke.Web.NewDDRMenu.DNNCommon;
    using DotNetNuke.Web.NewDDRMenu.TemplateEngine;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Dnn abstractions.</summary>
    internal static class DNNAbstract
    {
        /// <summary>Gets the Dnn login url.</summary>
        /// <returns>The url to login.</returns>
        public static string GetLoginUrl()
        {
            var request = HttpContext.Current.Request;
            if (request.IsAuthenticated)
            {
                var navigationManager = Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
                return navigationManager.NavigateURL(PortalSettings.Current.ActiveTab.TabID, "Logoff");
            }

            var returnUrl = HttpContext.Current.Request.RawUrl;
            if (returnUrl.IndexOf("?returnurl=") != -1)
            {
                returnUrl = returnUrl.Substring(0, returnUrl.IndexOf("?returnurl="));
            }

            returnUrl = HttpUtility.UrlEncode(returnUrl);

            return Globals.LoginURL(returnUrl, !string.IsNullOrEmpty(request.QueryString["override"]));
        }

        /// <summary>Gets a url to the user profile or or the registration page.</summary>
        /// <returns>If the user is logged in, returns the url to the user profile page, if not returns the url to the registration page.</returns>
        public static string GetUserUrl()
        {
            var request = HttpContext.Current.Request;
            var portalSettings = PortalSettings.Current;
            if (!request.IsAuthenticated)
            {
                if (portalSettings.UserRegistration != (int)Globals.PortalRegistrationType.NoRegistration)
                {
                    var navigationManager = Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
                    return Globals.RegisterURL(HttpUtility.UrlEncode(navigationManager.NavigateURL()), Null.NullString);
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

        /// <summary>Gets the current culture.</summary>
        /// <returns>The current culture code.</returns>
        public static string GetCurrentCulture()
        {
            return DNNContext.Current.PortalSettings.CultureCode;
        }

        /// <summary>Gets all the supported templating engines processors.</summary>
        /// <returns>An enumeration of all the available processors.</returns>
        public static IEnumerable<ITemplateProcessor> SupportedTemplateProcessors()
        {
            return new ITemplateProcessor[] { new TokenTemplateProcessor(), new RazorTemplateProcessor(), new XsltTemplateProcessor() };
        }

        /// <summary>Gets the navigation nodes options.</summary>
        /// <param name="includeHidden">A value indicating whether to include the hidden nodes.</param>
        /// <returns>An integer totalling the options values, <see cref="Navigation.NavNodeOptions"/> for the values.</returns>
        public static int GetNavNodeOptions(bool includeHidden)
        {
            return (int)Navigation.NavNodeOptions.IncludeSiblings + (int)Navigation.NavNodeOptions.IncludeSelf +
                   (includeHidden ? (int)Navigation.NavNodeOptions.IncludeHiddenNodes : 0);
        }

        /// <summary>Gets a value indicating whether it is supported to include hidden nodes.</summary>
        /// <returns>Always true.</returns>
        public static bool IncludeHiddenSupported()
        {
            return true;
        }

        /// <summary>Converts a <see cref="DNNNode"/> into a <see cref="MenuNode"/>.</summary>
        /// <param name="dnnNode">The DnnNode to convert.</param>
        /// <param name="menuNode">The MenuNode to return.</param>
        public static void DNNNodeToMenuNode(DNNNode dnnNode, MenuNode menuNode)
        {
            menuNode.LargeImage = dnnNode.LargeImage;
        }
    }
}
