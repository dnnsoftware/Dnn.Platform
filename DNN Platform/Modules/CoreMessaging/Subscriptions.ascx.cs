// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Modules.SubscriptionsMgmt.Components.Common;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Subscriptions.Controllers;
using DotNetNuke.Services.Subscriptions.Entities;
using DotNetNuke.UI.Modules;
using DotNetNuke.UI.Utilities;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;
using SubscriptionController = DotNetNuke.Modules.SubscriptionsMgmt.Components.Controllers.SubscriptionController;

namespace DotNetNuke.Modules.SubscriptionsMgmt
{
	public partial class Subscriptions : PortalModuleBase
	{
		#region Event Handlers

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

			if (Request.IsAuthenticated)
			{
				//SocialModule.RequestCommonCSS(Page);

				ClientResourceManager.RegisterScript(Page, "~/DesktopModules/CoreMessaging/Scripts/dnn.social.ExceptionHandlerDialog.js");
				ClientResourceManager.RegisterScript(Page, "~/DesktopModules/CoreMessaging/Scripts/dnn.social.Module.js");
				ClientResourceManager.RegisterScript(Page, "~/DesktopModules/CoreMessaging/Scripts/dnn.social.ComponentFactory.js");
				ClientResourceManager.RegisterScript(Page, "~/DesktopModules/CoreMessaging/Scripts/dnn.social.ServiceCaller.js");
				ClientResourceManager.RegisterScript(Page, "~/DesktopModules/CoreMessaging/Scripts/dnn.social.PagingControl.js");
				ClientResourceManager.RegisterScript(Page, "~/DesktopModules/CoreMessaging/Scripts/dnn.social.LocalizationController.js");
				ClientResourceManager.RegisterScript(Page, "~/DesktopModules/CoreMessaging/Scripts/dnn.social.Utilities.js");

				ClientResourceManager.RegisterScript(Page, "~/DesktopModules/CoreMessaging/Scripts/dnn.subscriptions.Subscription.js");
				ClientResourceManager.RegisterScript(Page, "~/DesktopModules/CoreMessaging/Scripts/dnn.subscriptions.SearchController.js");
				ClientResourceManager.RegisterScript(Page, "~/DesktopModules/CoreMessaging/Scripts/dnn.subscriptions.SearchResult.js");

                RequestClientScripts(Page, ModuleContext, "initSubscriptions", GetViewSettings(), Null.NullInteger);
			}
			else
			{
				Response.Redirect("AccessDenied", false);
			}
		}

		#endregion

		#region Private methods

		/// <summary>
		/// These values are passed in as the 'settings' parameter of the JavaScript initialization function, together with
		/// values that are automatically retrieved by Social Library such as portalId and moduleId.
		/// </summary>
		private object GetViewSettings()
		{
            var colInboxSubs = SubscriptionController.Instance.GetUserInboxSubscriptions(ModuleContext.PortalSettings.UserId, ModuleConfiguration.OwnerPortalID);
            var notifyFreq = 2;
            var msgFreq = 0;
            var notifySubId = -1;
            var msgSubId = -1;

            if (colInboxSubs.Count > 0)
            {
                foreach (var i in colInboxSubs)
                {
                    switch (i.FriendlyName)
                    {
                        case "Notifications":
                            notifyFreq = i.Frequency;
                            notifySubId = i.SubscriberId;
                            break;
                        case "Messages":
                            msgFreq = i.Frequency;
                            msgSubId = i.SubscriberId;
                            break;
                    }
                }
            }

			return new
			{
				moduleScope = string.Format("#{0}", ScopeWrapper.ClientID),
				pageSize = 25,
                notifySubscriberId = notifySubId,
                msgSubscriberId = msgSubId,
                notifyFrequency = notifyFreq,
                msgFrequency = msgFreq,
                notifySubTypeId = GetSubscriptionType("Notifications").SubscriptionTypeId,
                msgSubTypeId = GetSubscriptionType("Messages").SubscriptionTypeId
			};
		}

        private static SubscriptionType GetSubscriptionType(string subscriptionType)
        {
            return SubscriptionTypeController.Instance.GetTypeByName(-1, subscriptionType);
        }

		private void RequestClientScripts(Page page)
		{
			jQuery.RequestRegistration();
			jQuery.RequestUIRegistration();
			jQuery.RequestDnnPluginsRegistration();

			const FileOrder.Js order = FileOrder.Js.jQuery;

			ClientResourceManager.RegisterScript(page, "~/Resources/Shared/Scripts/knockout.js", order);
			ClientResourceManager.RegisterScript(page, "~/Resources/Shared/Scripts/jquery.history.js", order);
			ClientResourceManager.RegisterScript(page, "~/Resources/Shared/Scripts/json2.js", order);
		}

		private void RequestClientScripts(Page page, ModuleInstanceContext modContext, string clientInitializer, object additionalSettings, int uniqueId)
		{
			RequestClientScripts(page);

			if (additionalSettings == null)
			{
				additionalSettings = new object();
			}

			// If there are multiple familial modules on the page, or multiple instances of the same module,
			// we can get into a bad scenario where the init code doesn't run or food hadn't been made up,
			// she would basically just disappear. Later in life she became more attentive etc.eee
			foreach (var attemptId in new[] { modContext.TabModuleId, modContext.ModuleId, Rng.Next() })
			{
				var v = string.Format("social_init_{0}", attemptId);

				if (string.IsNullOrEmpty(ClientAPI.GetClientVariable(page, v)))
				{
					ClientAPI.RegisterClientVariable(page, v, "true", true);

					uniqueId = attemptId;
					break;
				}
			}

			var jsonSettings = GetSettingsJson(modContext.PortalSettings, modContext.Configuration, uniqueId).ToJson();
			var jsonAdditionalSettings = additionalSettings.ToJson();

			var script = new StringBuilder("<script type='text/javascript'>")
				.AppendLine("jQuery(function() {")
				.AppendLine(" 'use strict';")
				.AppendLine(string.Format(" var settings_{0} = {1};", uniqueId, jsonSettings))
				.AppendLine(string.Format(" jQuery.extend(settings_{0}, {1});", uniqueId, jsonAdditionalSettings))
				.AppendLine(string.Format(" if (typeof({0}) === 'function') {{", clientInitializer))
				.AppendLine(string.Format("  {0}.call(window, settings_{1});", clientInitializer, uniqueId))
				.AppendLine("  }")
				.AppendLine("});")
				.AppendLine("</script>");

			var scriptKey = string.Format("social_init_module_{0}_{1}", uniqueId, clientInitializer);

			ClientAPI.RegisterStartUpScript(page, scriptKey, script.ToString());
		}

		public static object GetSettingsJson(PortalSettings portalSettings, ModuleInfo moduleInfo, int uniqueId)
		{
			var usePopup =
				portalSettings.EnablePopUps &&
				portalSettings.LoginTabId == Null.NullInteger &&
				!HasSocialAuthenticationEnabled();

			var navigationKey =
				moduleInfo != null &&
				moduleInfo.DesktopModule != null
					? GetHistoryNavigationKey(moduleInfo.DesktopModule.FriendlyName)
					: null;

			var moduleRoot =
				moduleInfo != null &&
				moduleInfo.DesktopModule != null
					? moduleInfo.DesktopModule.FolderName
					: null;

			var moduleTitle = moduleInfo != null
				? moduleInfo.ModuleTitle
				: null;

			var moduleId = moduleInfo != null ? moduleInfo.ModuleID : Null.NullInteger;

			var moduleSettings = moduleInfo != null ? moduleInfo.ModuleSettings : new Hashtable();

			var debug = false;

#if DEBUG
			debug = true;
#else
            if (HttpContext.Current != null)
            {
                debug = HttpContext.Current.IsDebuggingEnabled;
            }
#endif

			return new
			{
				anonymous = PortalSettings.Current.UserId < 0,
				currentUserId = PortalSettings.Current.UserId,
				debug,
				culture = CultureInfo.CurrentUICulture.Name,
				showMissingKeys = Localization.ShowMissingKeys,
				portalId = portalSettings.PortalId,
				moduleId,
				moduleSettings,
				moduleTitle,
				moduleRoot,
				navigationKey,
				sessionTimeout = Convert.ToInt32(GetSessionTimeout().TotalMinutes),
				sharedResources = GetSharedResources(),
				authorizationUrl = GetLoginUrl(portalSettings),
				usePopup,
				returnUrl = HttpContext.Current.Request.UrlReferrer,
				uniqueId
			};
		}

		private static bool HasSocialAuthenticationEnabled()
		{
			return (from a in DotNetNuke.Services.Authentication.AuthenticationController.GetEnabledAuthenticationServices()
					let enabled = (a.AuthenticationType == "Facebook"
									 || a.AuthenticationType == "Google"
									 || a.AuthenticationType == "Live"
									 || a.AuthenticationType == "Twitter")
								  ? PortalController.GetPortalSettingAsBoolean(a.AuthenticationType + "_Enabled", PortalSettings.Current.PortalId, false)
								  : !string.IsNullOrEmpty(a.LoginControlSrc) //&& (LoadControl("~/" + a.LoginControlSrc) as AuthenticationLoginBase).Enabled
					where a.AuthenticationType != "DNN" && enabled
					select a).Any();
		}

		private static string GetHistoryNavigationKey(string moduleName)
		{
			return HttpContext.Current.Server.HtmlEncode(moduleName.ToLowerInvariant().Replace(" ", string.Empty));
		}

		private static TimeSpan GetSessionTimeout()
		{
			try
			{
				var sessionSection =
					WebConfigurationManager.GetSection("system.web/sessionState") as SessionStateSection;

				if (sessionSection != null)
				{
					return sessionSection.Timeout;
				}
			}
			catch
			{
				// FIXME(cbond): The default configuration doesn't actually let us see this data
				// FIXME(cbond): It's too annoying seeing this fill the Event Log, we need to add the permission to web.config
				// Exceptions.LogException(ex);
			}

			return TimeSpan.FromMinutes(25);
		}

		private static IDictionary<string, string> GetSharedResources()
		{
			return new Dictionary<string, string>
                {
                    {"ExceptionTitle", Localization.GetString("ExceptionTitle", Constants.SharedResources)},
                    {"ExceptionMessage", Localization.GetString("ExceptionMessage", Constants.SharedResources)}
                };
		}

		private static readonly Random Rng = new Random(DateTime.Now.Millisecond);

		private static string GetLoginUrl(PortalSettings portalSettings)
		{
			var returnUrl = HttpContext.Current.Request.RawUrl;

			if (portalSettings.UserId < 1)
			{
				var indexOf = returnUrl.IndexOf("?returnurl=", StringComparison.InvariantCultureIgnoreCase);
				if (indexOf >= 0)
				{
					returnUrl = returnUrl.Substring(0, indexOf);
				}

				returnUrl = Common.Globals.LoginURL(HttpUtility.UrlEncode(returnUrl), true);
			}

			return returnUrl;
		}

		#endregion
	}
}
