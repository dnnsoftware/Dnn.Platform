// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Caching;
    using System.Web.UI;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;

    using DataCache = DotNetNuke.UI.Utilities.DataCache;
    using Globals = DotNetNuke.Common.Globals;

#pragma warning disable 618
    /// <summary>Manages requests to add jQuery to a page.</summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

    // ReSharper disable once InconsistentNaming
    public partial class jQuery
    {
        /// <summary>Returns the default URL for a hosted version of the jQuery script.</summary>
        /// <remarks>
        /// Google hosts versions of many popular javascript libraries on their CDN.
        /// Using the hosted version increases the likelihood that the file is already
        /// cached in the user's browser.
        /// </remarks>
        public const string DefaultHostedUrl = "https://ajax.googleapis.com/ajax/libs/jquery/3.7.1/jquery.min.js";

        public const string DefaultUIHostedUrl = "https://ajax.googleapis.com/ajax/libs/jqueryui/1.13.3/jquery-ui.min.js";
        private const string JQueryDebugFile = "~/Resources/Shared/Scripts/jquery/jquery.js";
        private const string JQueryMinFile = "~/Resources/Shared/Scripts/jquery/jquery.min.js";
        private const string JQueryMigrateDebugFile = "~/Resources/Shared/Scripts/jquery/jquery-migrate.js";
        private const string JQueryMigrateMinFile = "~/Resources/Shared/Scripts/jquery/jquery-migrate.min.js";
        private const string JQueryVersionKey = "jQueryVersionKey";
        private const string JQueryVersionMatch = "(?<=(jquery|core_version)\\s*[:=]\\s*\")(.*)(?=\")";

        private const string JQueryUIDebugFile = "~/Resources/Shared/Scripts/jquery/jquery-ui.js";
        private const string JQueryUIMinFile = "~/Resources/Shared/Scripts/jquery/jquery-ui.min.js";
        private const string JQueryUIVersionKey = "jQueryUIVersionKey";
        private const string JQueryUIVersionMatch = "(?<=version:\\s\")(.*)(?=\")";

        public static bool IsRequested
        {
            get
            {
                return GetSettingAsBoolean("jQueryRequested", false);
            }
        }

        public static bool IsUIRequested
        {
            get
            {
                return GetSettingAsBoolean("jQueryUIRequested", false);
            }
        }

        public static bool AreDnnPluginsRequested
        {
            get
            {
                return GetSettingAsBoolean("jQueryDnnPluginsRequested", false);
            }
        }

        public static bool IsHoverIntentRequested
        {
            get
            {
                return GetSettingAsBoolean("jQueryHoverIntentRequested", false);
            }
        }

        /// <summary>Active the page with keep alive, so that authentication will not expire.</summary>
        /// <param name="page">The page instance.</param>
        public static void KeepAlive(Page page)
        {
            var cookieTimeout = Config.GetAuthCookieTimeout();
            if (cookieTimeout <= 0 || page.ClientScript.IsClientScriptBlockRegistered("PageKeepAlive"))
            {
                return;
            }

            if (cookieTimeout > 5)
            {
                cookieTimeout = 5; // ping server in 5 minutes to make sure the server is not IDLE.
            }

            JavaScript.RequestRegistration(CommonJs.jQuery);

            var seconds = ((cookieTimeout * 60) - 30) * 1000; // ping server 30 seconds before cookie is time out.
            var scriptBlock = string.Format(CultureInfo.InvariantCulture, "(function($){{setInterval(function(){{$.get(location.href)}}, {1});}}(jQuery));", Globals.ApplicationPath, seconds);
            ScriptManager.RegisterClientScriptBlock(page, page.GetType(), "PageKeepAlive", scriptBlock, true);
        }

        private static bool GetSettingAsBoolean(string key, bool defaultValue)
        {
            bool retValue = defaultValue;
            try
            {
                object setting = HttpContext.Current.Items[key];
                if (setting != null)
                {
                    retValue = Convert.ToBoolean(setting, CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            return retValue;
        }
    }
#pragma warning restore 618
}
