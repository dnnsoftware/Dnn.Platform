#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Utilities;
using DotNetNuke.Web.Client.ClientResourceManagement;

using DataCache = DotNetNuke.UI.Utilities.DataCache;
using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DotNetNuke.Framework
{
#pragma warning disable 618
	public class jQuery
    {
        private const string jQueryDebugFile = "~/Resources/Shared/Scripts/jquery/jquery.js";
        private const string jQueryMinFile = "~/Resources/Shared/Scripts/jquery/jquery.min.js";
		private const string jQueryMigrateDebugFile = "~/Resources/Shared/Scripts/jquery/jquery-migrate.js";
		private const string jQueryMigrateMinFile = "~/Resources/Shared/Scripts/jquery/jquery-migrate.min.js";
        private const string jQueryVersionKey = "jQueryVersionKey";
		private const string jQueryVersionMatch = "(?<=(jquery|core_version)\\s*[:=]\\s*\")(.*)(?=\")";

        /// <summary>
        /// Returns the default URL for a hosted version of the jQuery script
        /// </summary>
        /// <remarks>
        /// Google hosts versions of many popular javascript libraries on their CDN.
        /// Using the hosted version increases the likelihood that the file is already
        /// cached in the users browser.
        /// </remarks>
        public const string DefaultHostedUrl = "http://ajax.googleapis.com/ajax/libs/jquery/3.2.1/jquery.min.js";

        private const string jQueryUIDebugFile = "~/Resources/Shared/Scripts/jquery/jquery-ui.js";
        private const string jQueryUIMinFile = "~/Resources/Shared/Scripts/jquery/jquery-ui.min.js";
        private const string jQueryUIVersionKey = "jQueryUIVersionKey";
        private const string jQueryUIVersionMatch = "(?<=version:\\s\")(.*)(?=\")";
        public const string DefaultUIHostedUrl = "http://ajax.googleapis.com/ajax/libs/jqueryui/1.10.3/jquery-ui.min.js";

        #region Public Properties

        /// <summary>
        /// Gets the HostSetting for the URL of the hosted version of the jQuery script.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>This is a simple wrapper around the Host.jQueryUrl property</remarks>
        [Obsolete("This is managed through the JavaScript Library package. Scheduled removal in v10.0.0.")]
        public static string HostedUrl
        {
            get
            {
                if (Globals.Status != Globals.UpgradeStatus.None)
                {
                    return String.Empty;
                }

                return Host.jQueryUrl;
            }
        }

		/// <summary>
		/// Gets the HostSetting for the URL of the hosted version of the jQuery migrated script.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks>This is a simple wrapper around the Host.jQueryUrl property</remarks>
        [Obsolete("This is managed through the JavaScript Library package. Scheduled removal in v10.0.0.")]
        public static string HostedMigrateUrl
		{
			get
			{
				if (Globals.Status != Globals.UpgradeStatus.None)
				{
					return String.Empty;
				}

				return Host.jQueryMigrateUrl;
			}
		}

        /// <summary>
        /// Gets the HostSetting for the URL of the hosted version of the jQuery UI script.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>This is a simple wrapper around the Host.jQueryUIUrl property</remarks>
        [Obsolete("This is managed through the JavaScript Library package. Scheduled removal in v10.0.0.")]
        public static string HostedUIUrl
        {
            get
            {
                if (Globals.Status != Globals.UpgradeStatus.None)
                {
                    return String.Empty;
                }

                return Host.jQueryUIUrl;
            }
        }

        /// <summary>
        /// Checks whether the jQuery core script file exists locally.
        /// </summary>
        /// <remarks>
        /// This property checks for both the minified version and the full uncompressed version of jQuery.
        /// These files should exist in the /Resources/Shared/Scripts/jquery directory.
        /// </remarks>
        [Obsolete("This is managed through the JavaScript Library package. Scheduled removal in v10.0.0.")]
        public static bool IsInstalled
        {
            get
            {
                string minFile = JQueryFileMapPath(true);
                string dbgFile = JQueryFileMapPath(false);
                return File.Exists(minFile) || File.Exists(dbgFile);
            }
        }

        /// <summary>
        /// Checks whether the jQuery UI core script file exists locally.
        /// </summary>
        /// <remarks>
        /// This property checks for both the minified version and the full uncompressed version of jQuery UI.
        /// These files should exist in the /Resources/Shared/Scripts/jquery directory.
        /// </remarks>
        [Obsolete("This is managed through the JavaScript Library package. Scheduled removal in v10.0.0.")]
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

        /// <summary>
        /// Gets the HostSetting to determine if we should use the standard jQuery script or the minified jQuery script.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>This is a simple wrapper around the Host.jQueryDebug property</remarks>
        [Obsolete("This is managed through the JavaScript Library package. Scheduled removal in v10.0.0.")]
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

        /// <summary>
        /// Gets the HostSetting to determine if we should use a hosted version of the jQuery script.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>This is a simple wrapper around the Host.jQueryHosted property</remarks>
        [Obsolete("This is managed through the JavaScript Library package. Scheduled removal in v10.0.0.")]
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

        /// <summary>
        /// Gets the version string for the local jQuery script
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>
        /// This only evaluates the version in the full jQuery file and assumes that the minified script
        /// is the same version as the full script.
        /// </remarks>
        public static string Version
        {
            get
            {
                string ver = Convert.ToString(DataCache.GetCache(jQueryVersionKey));
                if (string.IsNullOrEmpty(ver))
                {
                    if (IsInstalled)
                    {
                        string jqueryFileName = JQueryFileMapPath(false);
                        string jfiletext = File.ReadAllText(jqueryFileName);
                        Match verMatch = Regex.Match(jfiletext, jQueryVersionMatch);
                        ver = verMatch.Value;
                        DataCache.SetCache(jQueryVersionKey, ver, new CacheDependency(jqueryFileName));
                    }
                    else
                    {
                        ver = Localization.GetString("jQuery.NotInstalled.Text");
                    }
                }
                return ver;
            }
        }

        /// <summary>
        /// Gets the version string for the local jQuery UI script
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>
        /// This only evaluates the version in the full jQuery UI file and assumes that the minified script
        /// is the same version as the full script.
        /// </remarks>
        public static string UIVersion
        {
            get
            {
                string ver = Convert.ToString(DataCache.GetCache(jQueryUIVersionKey));
                if (string.IsNullOrEmpty(ver))
                {
                    if (IsUIInstalled)
                    {
                        string jqueryUIFileName = JQueryUIFileMapPath(false);
                        string jfiletext = File.ReadAllText(jqueryUIFileName);
                        Match verMatch = Regex.Match(jfiletext, jQueryUIVersionMatch);
                        ver = verMatch.Value;
                        DataCache.SetCache(jQueryUIVersionKey, ver, new CacheDependency(jqueryUIFileName));
                    }
                    else
                    {
                        ver = Localization.GetString("jQueryUI.NotInstalled.Text");
                    }
                }
                return ver;
            }
        }
        #endregion

        #region Private Methods

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

        #endregion

        #region Public Methods

        [Obsolete("This is managed through the JavaScript Library package. Scheduled removal in v10.0.0.")]
        public static string JQueryFileMapPath(bool getMinFile)
        {
            return HttpContext.Current.Server.MapPath(JQueryFile(getMinFile));
        }

        [Obsolete("This is managed through the JavaScript Library package. Scheduled removal in v10.0.0.")]
        public static string JQueryUIFileMapPath(bool getMinFile)
        {
            return HttpContext.Current.Server.MapPath(JQueryUIFile(getMinFile));
        }

        [Obsolete("This is managed through the JavaScript Library package. Scheduled removal in v10.0.0.")]
        public static string JQueryFile(bool getMinFile)
        {
            string jfile = jQueryDebugFile;
            if (getMinFile)
            {
                jfile = jQueryMinFile;
            }
            return jfile;
        }

        [Obsolete("This is managed through the JavaScript Library package. Scheduled removal in v10.0.0.")]
        public static string JQueryMigrateFile(bool getMinFile)
		{
			string jfile = jQueryMigrateDebugFile;
			if (getMinFile)
			{
				jfile = jQueryMigrateMinFile;
			}
			return jfile;
		}

        [Obsolete("This is managed through the JavaScript Library package. Scheduled removal in v10.0.0.")]
        public static string JQueryUIFile(bool getMinFile)
        {
            string jfile = jQueryUIDebugFile;
            if (getMinFile)
            {
                jfile = jQueryUIMinFile;
            }
            return jfile;
        }

        [Obsolete("This is managed through the JavaScript Library package. Scheduled removal in v10.0.0.")]
        public static string GetJQueryScriptReference()
        {
            string scriptsrc = HostedUrl;
            if (!UseHostedScript)
            {
                scriptsrc = JQueryFile(!UseDebugScript);
            }
            return scriptsrc;
        }

        [Obsolete("This is managed through the JavaScript Library package. Scheduled removal in v10.0.0.")]
        public static string GetJQueryMigrateScriptReference()
		{
			string scriptsrc = HostedMigrateUrl;
			if (!UseHostedScript || string.IsNullOrEmpty(scriptsrc))
			{
				scriptsrc = JQueryMigrateFile(!UseDebugScript);
			}
			return scriptsrc;
		}

        [Obsolete("This is managed through the JavaScript Library package. Scheduled removal in v10.0.0.")]
        public static string GetJQueryUIScriptReference()
        {
            string scriptsrc = HostedUIUrl;
            if (!UseHostedScript)
            {
                scriptsrc = JQueryUIFile(!UseDebugScript);
            }
            return scriptsrc;
        }



		/// <summary>
		/// Active the page with keep alive, so that authentication will not expire.
		/// </summary>
		/// <param name="page">The page instance.</param>
		public static void KeepAlive(Page page)
		{
			var cookieTimeout = Config.GetAuthCookieTimeout();
			if(cookieTimeout <= 0 || page.ClientScript.IsClientScriptBlockRegistered("PageKeepAlive"))
			{
				return;
			}

			if(cookieTimeout > 5)
			{
				cookieTimeout = 5; // ping server in 5 minutes to make sure the server is not IDLE.
			}
            JavaScript.RequestRegistration(CommonJs.jQuery);

			var seconds = (cookieTimeout*60 - 30)*1000; //ping server 30 seconds before cookie is time out.
			var scriptBlock = string.Format("(function($){{setInterval(function(){{$.get(location.href)}}, {1});}}(jQuery));", Globals.ApplicationPath, seconds);
			ScriptManager.RegisterClientScriptBlock(page, page.GetType(), "PageKeepAlive", scriptBlock, true);
		}

        #endregion

        #region Obsolete Members

        [Obsolete("Obsoleted in 7.2.0 - registration occurs automatically during page load. Scheduled removal in v10.0.0.")]
        public static void RegisterJQuery(Page page)
        {
            JavaScript.RequestRegistration(CommonJs.jQuery);
            JavaScript.RequestRegistration(CommonJs.jQueryMigrate);
        }

        [Obsolete("Obsoleted in 7.2.0 - registration occurs automatically during page load. Scheduled removal in v10.0.0.")]
        public static void RegisterJQueryUI(Page page)
        {
            RegisterJQuery(page);
            JavaScript.RequestRegistration(CommonJs.jQueryUI);
        }

        [Obsolete("Obsoleted in 7.2.0 - registration occurs automatically during page load. Scheduled removal in v10.0.0.")]
        public static void RegisterDnnJQueryPlugins(Page page)
        {
            RegisterJQueryUI(page);
            RegisterHoverIntent(page);
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
        }

        [Obsolete("Obsoleted in 7.2.0 - registration occurs automatically during page load. Scheduled removal in v10.0.0.")]
        public static void RegisterHoverIntent(Page page)
        {
            JavaScript.RequestRegistration(CommonJs.HoverIntent);
        }

        public static void RegisterFileUpload(Page page)
        {
            JavaScript.RequestRegistration(CommonJs.jQueryFileUpload);

        }

        [Obsolete("Obsoleted in 7.2.0 - use JavaScript.RequestRegistration(CommonJs.jQuery);. Scheduled removal in v10.0.0.")]
        public static void RequestRegistration()
        {

            JavaScript.RequestRegistration(CommonJs.jQuery);
            JavaScript.RequestRegistration(CommonJs.jQueryMigrate);
        }

        [Obsolete("Obsoleted in 7.2.0 - use JavaScript.RequestRegistration(CommonJs.jQueryUI);. Scheduled removal in v10.0.0.")]
        public static void RequestUIRegistration()
        {

            JavaScript.RequestRegistration(CommonJs.jQueryUI);
        }

        [Obsolete("Obsoleted in 7.2.0 - use JavaScript.RequestRegistration(CommonJs.DnnPlugins);. Scheduled removal in v10.0.0.")]
        public static void RequestDnnPluginsRegistration()
        {

            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
        }

        [Obsolete("Obsoleted in 7.2.0 - use JavaScript.RequestRegistration(CommonJs.HoverIntent);. Scheduled removal in v10.0.0.")]
        public static void RequestHoverIntentRegistration()
        {

            JavaScript.RequestRegistration(CommonJs.HoverIntent);
        }


        #endregion
	}
#pragma warning restore 618
}
