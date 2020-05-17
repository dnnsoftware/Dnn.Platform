// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
    [Obsolete("Managed via Libraries, Scheduled for Removal in v11.0.0")]
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


        public static void RegisterFileUpload(Page page)
        {
            JavaScript.RequestRegistration(CommonJs.jQueryFileUpload);

        }
        

        #endregion
	}
#pragma warning restore 618
}
