// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Web;
    using System.Web.Mvc;
    using System.Xml;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.MvcPipeline.Models;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Skin helper methods for rendering toast notification scripts.
    /// </summary>
    public static partial class SkinHelpers
    {
        private static readonly string ToastCacheKey = "DNN_Toast_Config";

        /// <summary>
        /// Renders the client-side script required to show toast notifications for the current user.
        /// </summary>
        /// <param name="helper">The HTML helper for the current <see cref="PageModel"/>.</param>
        /// <param name="cssClass">Unused parameter preserved for API compatibility.</param>
        /// <returns>An HTML string containing the toast initialization script, or empty if the user is offline.</returns>
        public static IHtmlString Toast(this HtmlHelper<PageModel> helper, string cssClass = "SkinObject")
        {
            if (!IsOnline())
            {
                return MvcHtmlString.Empty;
            }

            var portalSettings = PortalSettings.Current;

            // Register Resources
            var javaScript = Globals.GetCurrentServiceProvider().GetRequiredService<IJavaScriptLibraryHelper>();
            javaScript.RequestRegistration(CommonJs.jQueryUI);

            var clientResourceController = Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
            clientResourceController.RegisterScript("~/Resources/Shared/components/Toast/jquery.toastmessage.js", FileOrder.Js.jQuery);
            clientResourceController.RegisterStylesheet("~/Resources/Shared/components/Toast/jquery.toastmessage.css", FileOrder.Css.DefaultCss);

            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            var config = InitializeToastConfig();
            string serviceModuleName = config["ServiceModuleName"];
            string serviceAction = config["ServiceAction"];
            string additionalScripts = config.ContainsKey("AddtionalScripts") ? config["AddtionalScripts"] : string.Empty;

            string notificationLink = GetNotificationLink(portalSettings);
            string notificationLabel = GetNotificationLabel();

            var script = $@"
<script type=""text/javascript"">
    $(document).ready(function () {{
        if (typeof dnn == 'undefined') dnn = {{}};
        if (typeof dnn.toast == 'undefined') dnn.toast = {{}};
        var sf = $.ServicesFramework();

        dnn.toast.refreshUser = function () {{
            $.ajax({{
                type: ""GET"",
                url: sf.getServiceRoot('{serviceModuleName}') + '{serviceAction}',
                contentType: ""application/json"",
                dataType: ""json"",
                cache: false,
                success: function (data) {{
                    if (typeof dnn.toast.toastTimer !== 'undefined') {{
                        clearTimeout(dnn.toast.toastTimer);
                        delete dnn.toast.toastTimer;
                    }}

                    if (!data || !data.Success) {{
                        return;
                    }}

                    $(document).trigger('dnn.toastupdate', data);

                    var toastMessages = [];

                    for (var i = 0; i < data.Toasts.length; i++) {{
                        var toast = {{
                            subject: data.Toasts[i].Subject,
                            body: data.Toasts[i].Body
                        }};

                        toastMessages.push(toast);
                    }}

                    var message = {{
                        messages: toastMessages,
                        seeMoreLink: '{notificationLink}', 
                        seeMoreText: '{Localization.GetSafeJSString(notificationLabel)}'
                    }};

                    $().dnnToastMessage('showAllToasts', message);

                    dnn.toast.toastTimer = setTimeout(dnn.toast.refreshUser, 30000);
                }},
                error: function (xhr, status, error) {{
                    if (typeof dnn.toast.toastTimer !== 'undefined') {{
                        clearTimeout(dnn.toast.toastTimer);
                        delete dnn.toast.toastTimer;
                    }}
                }}
            }});
        }};

        var pageUnloaded = window.dnnModal && window.dnnModal.pageUnloaded;
        if (!pageUnloaded) {{
            dnn.toast.toastTimer = setTimeout(dnn.toast.refreshUser, 4000);
        }}
    }});
</script>
{additionalScripts}";

            return new MvcHtmlString(script);
        }

        private static bool IsOnline()
        {
            var userInfo = UserController.Instance.GetCurrentUserInfo();
            return userInfo.UserID != -1;
        }

        private static string GetNotificationLink(PortalSettings portalSettings)
        {
            return GetMessageLink(portalSettings) + "?view=notifications&action=notifications";
        }

        private static string GetMessageLink(PortalSettings portalSettings)
        {
            var navigationManager = Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
            return navigationManager.NavigateURL(GetMessageTab(portalSettings), string.Empty, string.Format("userId={0}", portalSettings.UserId));
        }

        private static string GetNotificationLabel()
        {
            return Localization.GetString("SeeAllNotification", GetSkinsResourceFile("Toast.ascx"));
        }

        private static IDictionary<string, string> InitializeToastConfig()
        {
            var config = new Dictionary<string, string>
            {
                { "ServiceModuleName", "InternalServices" },
                { "ServiceAction", "NotificationsService/GetToasts" },
            };

            try
            {
                var toastConfig = DataCache.GetCache<IDictionary<string, string>>(ToastCacheKey);
                if (toastConfig != null)
                {
                    return toastConfig;
                }

                // Try to find Toast.config in admin/skins
                var configFile = HttpContext.Current.Server.MapPath("~/admin/Skins/Toast.config");

                if (File.Exists(configFile))
                {
                    var xmlDocument = new XmlDocument { XmlResolver = null };
                    xmlDocument.Load(configFile);
                    var moduleNameNode = xmlDocument.DocumentElement?.SelectSingleNode("moduleName");
                    var actionNode = xmlDocument.DocumentElement?.SelectSingleNode("action");
                    var scriptsNode = xmlDocument.DocumentElement?.SelectSingleNode("scripts");

                    if (moduleNameNode != null && !string.IsNullOrEmpty(moduleNameNode.InnerText))
                    {
                        config["ServiceModuleName"] = moduleNameNode.InnerText;
                    }

                    if (actionNode != null && !string.IsNullOrEmpty(actionNode.InnerText))
                    {
                        config["ServiceAction"] = actionNode.InnerText;
                    }

                    if (scriptsNode != null && !string.IsNullOrEmpty(scriptsNode.InnerText))
                    {
                        config["AddtionalScripts"] = scriptsNode.InnerText;
                    }
                }

                DataCache.SetCache(ToastCacheKey, config);
            }
            catch (Exception)
            {
                // DotNetNuke.Instrumentation.LoggerSource.Instance.GetLogger(typeof(SkinHelpers)).Error(ex);
            }

            return config;
        }
    }
}
