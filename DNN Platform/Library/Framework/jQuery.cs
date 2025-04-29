// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework;

using System;
using System.Diagnostics.CodeAnalysis;
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

    /// <summary>Gets the HostSetting for the URL of the hosted version of the jQuery script.</summary>
    /// <value>The HostSetting for the URL of the hosted version of the jQuery script.</value>
    /// <remarks>This is a simple wrapper around the Host.jQueryUrl property.</remarks>
    [Obsolete("Deprecated in DotNetNuke 7.3.1. This is managed through the JavaScript Library package. Scheduled removal in v10.0.0.")]
    public static string HostedUrl
    {
        get
        {
            if (Globals.Status != Globals.UpgradeStatus.None)
            {
                return string.Empty;
            }

            return Host.jQueryUrl;
        }
    }

    /// <summary>Gets the HostSetting for the URL of the hosted version of the jQuery migrated script.</summary>
    /// <value>The HostSetting for the URL of the hosted version of the jQuery migrated script.</value>
    /// <remarks>This is a simple wrapper around the Host.jQueryUrl property.</remarks>
    [Obsolete("Deprecated in DotNetNuke 7.3.1. This is managed through the JavaScript Library package. Scheduled removal in v10.0.0.")]
    public static string HostedMigrateUrl
    {
        get
        {
            if (Globals.Status != Globals.UpgradeStatus.None)
            {
                return string.Empty;
            }

            return Host.jQueryMigrateUrl;
        }
    }

    /// <summary>Gets the HostSetting for the URL of the hosted version of the jQuery UI script.</summary>
    /// <value>The HostSetting for the URL of the hosted version of the jQuery UI script.</value>
    /// <remarks>This is a simple wrapper around the Host.jQueryUIUrl property.</remarks>
    [Obsolete("Deprecated in DotNetNuke 7.3.1. This is managed through the JavaScript Library package. Scheduled removal in v10.0.0.")]
    public static string HostedUIUrl
    {
        get
        {
            if (Globals.Status != Globals.UpgradeStatus.None)
            {
                return string.Empty;
            }

            return Host.jQueryUIUrl;
        }
    }

    /// <summary>Gets a value indicating whether checks whether the jQuery core script file exists locally.</summary>
    /// <remarks>
    /// This property checks for both the minified version and the full uncompressed version of jQuery.
    /// These files should exist in the /Resources/Shared/Scripts/jquery directory.
    /// </remarks>
    [Obsolete("Deprecated in DotNetNuke 7.3.1. This is managed through the JavaScript Library package. Scheduled removal in v10.0.0.")]
    public static bool IsInstalled
    {
        get
        {
            string minFile = JQueryFileMapPath(true);
            string dbgFile = JQueryFileMapPath(false);
            return File.Exists(minFile) || File.Exists(dbgFile);
        }
    }

    /// <summary>Gets a value indicating whether checks whether the jQuery UI core script file exists locally.</summary>
    /// <remarks>
    /// This property checks for both the minified version and the full uncompressed version of jQuery UI.
    /// These files should exist in the /Resources/Shared/Scripts/jquery directory.
    /// </remarks>
    [Obsolete("Deprecated in DotNetNuke 7.3.1. This is managed through the JavaScript Library package. Scheduled removal in v10.0.0.")]
    public static bool IsUIInstalled
    {
        get
        {
            string minFile = JQueryUIFileMapPath(true);
            string dbgFile = JQueryUIFileMapPath(false);
            return File.Exists(minFile) || File.Exists(dbgFile);
        }
    }

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

    /// <summary>Gets a value indicating whether gets the HostSetting to determine if we should use the standard jQuery script or the minified jQuery script.</summary>
    /// <value>The HostSetting to determine if we should use the standard jQuery script or the minified jQuery script.</value>
    /// <remarks>This is a simple wrapper around the Host.jQueryDebug property.</remarks>
    [Obsolete("Deprecated in DotNetNuke 7.3.1. This is managed through the JavaScript Library package. Scheduled removal in v10.0.0.")]
    public static bool UseDebugScript
    {
        get
        {
            if (Globals.Status != Globals.UpgradeStatus.None)
            {
                return false;
            }

            return Host.jQueryDebug;
        }
    }

    /// <summary>Gets a value indicating whether gets the HostSetting to determine if we should use a hosted version of the jQuery script.</summary>
    /// <value>The HostSetting to determine if we should use a hosted version of the jQuery script.</value>
    /// <remarks>This is a simple wrapper around the Host.jQueryHosted property.</remarks>
    [Obsolete("Deprecated in DotNetNuke 7.3.1. This is managed through the JavaScript Library package. Scheduled removal in v10.0.0.")]
    public static bool UseHostedScript
    {
        get
        {
            if (Globals.Status != Globals.UpgradeStatus.None)
            {
                return false;
            }

            return Host.jQueryHosted;
        }
    }

    /// <summary>Gets the version string for the local jQuery script.</summary>
    /// <value>The version string for the local jQuery script.</value>
    /// <remarks>
    /// This only evaluates the version in the full jQuery file and assumes that the minified script
    /// is the same version as the full script.
    /// </remarks>
    public static string Version
    {
        get
        {
            string ver = Convert.ToString(DataCache.GetCache(JQueryVersionKey));
            if (string.IsNullOrEmpty(ver))
            {
                if (IsInstalled)
                {
                    string jqueryFileName = JQueryFileMapPath(false);
                    string jfiletext = File.ReadAllText(jqueryFileName);
                    Match verMatch = Regex.Match(jfiletext, JQueryVersionMatch);
                    ver = verMatch.Value;
                    DataCache.SetCache(JQueryVersionKey, ver, new CacheDependency(jqueryFileName));
                }
                else
                {
                    ver = Localization.GetString("jQuery.NotInstalled.Text");
                }
            }

            return ver;
        }
    }

    /// <summary>Gets the version string for the local jQuery UI script.</summary>
    /// <value>The version string for the local jQuery UI script.</value>
    /// <remarks>
    /// This only evaluates the version in the full jQuery UI file and assumes that the minified script
    /// is the same version as the full script.
    /// </remarks>
    public static string UIVersion
    {
        get
        {
            string ver = Convert.ToString(DataCache.GetCache(JQueryUIVersionKey));
            if (string.IsNullOrEmpty(ver))
            {
                if (IsUIInstalled)
                {
                    string jqueryUIFileName = JQueryUIFileMapPath(false);
                    string jfiletext = File.ReadAllText(jqueryUIFileName);
                    Match verMatch = Regex.Match(jfiletext, JQueryUIVersionMatch);
                    ver = verMatch.Value;
                    DataCache.SetCache(JQueryUIVersionKey, ver, new CacheDependency(jqueryUIFileName));
                }
                else
                {
                    ver = Localization.GetString("jQueryUI.NotInstalled.Text");
                }
            }

            return ver;
        }
    }

    [DnnDeprecated(7, 3, 1, "This is managed through the JavaScript Library package", RemovalVersion = 10)]
    public static partial string JQueryFileMapPath(bool getMinFile)
    {
        return HttpContext.Current.Server.MapPath(JQueryFile(getMinFile));
    }

    [DnnDeprecated(7, 3, 1, "This is managed through the JavaScript Library package", RemovalVersion = 10)]
    public static partial string JQueryUIFileMapPath(bool getMinFile)
    {
        return HttpContext.Current.Server.MapPath(JQueryUIFile(getMinFile));
    }

    [DnnDeprecated(7, 3, 1, "This is managed through the JavaScript Library package", RemovalVersion = 10)]
    public static partial string JQueryFile(bool getMinFile)
    {
        string jfile = JQueryDebugFile;
        if (getMinFile)
        {
            jfile = JQueryMinFile;
        }

        return jfile;
    }

    [DnnDeprecated(7, 3, 1, "This is managed through the JavaScript Library package", RemovalVersion = 10)]
    public static partial string JQueryMigrateFile(bool getMinFile)
    {
        string jfile = JQueryMigrateDebugFile;
        if (getMinFile)
        {
            jfile = JQueryMigrateMinFile;
        }

        return jfile;
    }

    [DnnDeprecated(7, 3, 1, "This is managed through the JavaScript Library package", RemovalVersion = 10)]
    public static partial string JQueryUIFile(bool getMinFile)
    {
        string jfile = JQueryUIDebugFile;
        if (getMinFile)
        {
            jfile = JQueryUIMinFile;
        }

        return jfile;
    }

    [DnnDeprecated(7, 3, 1, "This is managed through the JavaScript Library package", RemovalVersion = 10)]
    public static partial string GetJQueryScriptReference()
    {
        string scriptsrc = HostedUrl;
        if (!UseHostedScript)
        {
            scriptsrc = JQueryFile(!UseDebugScript);
        }

        return scriptsrc;
    }

    [DnnDeprecated(7, 3, 1, "This is managed through the JavaScript Library package", RemovalVersion = 10)]
    public static partial string GetJQueryMigrateScriptReference()
    {
        string scriptsrc = HostedMigrateUrl;
        if (!UseHostedScript || string.IsNullOrEmpty(scriptsrc))
        {
            scriptsrc = JQueryMigrateFile(!UseDebugScript);
        }

        return scriptsrc;
    }

    [DnnDeprecated(7, 3, 1, "This is managed through the JavaScript Library package", RemovalVersion = 10)]
    public static partial string GetJQueryUIScriptReference()
    {
        string scriptsrc = HostedUIUrl;
        if (!UseHostedScript)
        {
            scriptsrc = JQueryUIFile(!UseDebugScript);
        }

        return scriptsrc;
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
        var scriptBlock = string.Format("(function($){{setInterval(function(){{$.get(location.href)}}, {1});}}(jQuery));", Globals.ApplicationPath, seconds);
        ScriptManager.RegisterClientScriptBlock(page, page.GetType(), "PageKeepAlive", scriptBlock, true);
    }

    [DnnDeprecated(7, 2, 0, "Registration occurs automatically during page load", RemovalVersion = 10)]
    public static partial void RegisterJQuery(Page page)
    {
        JavaScript.RequestRegistration(CommonJs.jQuery);
        JavaScript.RequestRegistration(CommonJs.jQueryMigrate);
    }

    [DnnDeprecated(7, 2, 0, "Registration occurs automatically during page load", RemovalVersion = 10)]
    public static partial void RegisterJQueryUI(Page page)
    {
        RegisterJQuery(page);
        JavaScript.RequestRegistration(CommonJs.jQueryUI);
    }

    [DnnDeprecated(7, 2, 0, "Registration occurs automatically during page load", RemovalVersion = 10)]
    public static partial void RegisterDnnJQueryPlugins(Page page)
    {
        RegisterJQueryUI(page);
        RegisterHoverIntent(page);
        JavaScript.RequestRegistration(CommonJs.DnnPlugins);
    }

    [DnnDeprecated(7, 2, 0, "Use JavaScript.RequestRegistration(CommonJs.HoverIntent)", RemovalVersion = 10)]
    public static partial void RegisterHoverIntent(Page page)
    {
        JavaScript.RequestRegistration(CommonJs.HoverIntent);
    }

    [DnnDeprecated(7, 2, 0, "Use JavaScript.RequestRegistration(CommonJs.jQueryFileUpload)", RemovalVersion = 10)]
    public static partial void RegisterFileUpload(Page page)
    {
        JavaScript.RequestRegistration(CommonJs.jQueryFileUpload);
    }

    [DnnDeprecated(7, 2, 0, "Use JavaScript.RequestRegistration(CommonJs.jQuery)", RemovalVersion = 10)]
    public static partial void RequestRegistration()
    {
        JavaScript.RequestRegistration(CommonJs.jQuery);
        JavaScript.RequestRegistration(CommonJs.jQueryMigrate);
    }

    [DnnDeprecated(7, 2, 0, "Use JavaScript.RequestRegistration(CommonJs.jQueryUI)", RemovalVersion = 10)]
    public static partial void RequestUIRegistration()
    {
        JavaScript.RequestRegistration(CommonJs.jQueryUI);
    }

    [DnnDeprecated(7, 2, 0, "Use JavaScript.RequestRegistration(CommonJs.DnnPlugins)", RemovalVersion = 10)]
    public static partial void RequestDnnPluginsRegistration()
    {
        JavaScript.RequestRegistration(CommonJs.DnnPlugins);
    }

    [DnnDeprecated(7, 2, 0, "Use JavaScript.RequestRegistration(CommonJs.HoverIntent)", RemovalVersion = 10)]
    public static partial void RequestHoverIntentRegistration()
    {
        JavaScript.RequestRegistration(CommonJs.HoverIntent);
    }

    private static bool GetSettingAsBoolean(string key, bool defaultValue)
    {
        bool retValue = defaultValue;
        try
        {
            object setting = HttpContext.Current.Items[key];
            if (setting != null)
            {
                retValue = Convert.ToBoolean(setting);
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
